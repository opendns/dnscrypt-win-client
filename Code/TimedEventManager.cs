using System;
using System.Collections.Generic;
using System.Text;

using System.Threading;

using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

using System.IO;

using System.Management;

using System.Diagnostics;

using System.Reflection;

// Handles state and all the things that get checked on a timer in the main
// service code
public sealed class TimedEventManager
{
    #region Enums

    public enum RUN_STATE { RESERVED = 0, SERVICE_RUNNING, SERVICE_RESTART, DNSCRYPT_FULL, DNSCRYPT_TCP, OPENDNS_ONLY, FAIL_OPEN, FAIL_CLOSED };
    public enum PORT_STATE { RESERVED = 0, NONE, PRIMARY, ALTERNATE };
    public enum NETWORK_STATE { RESERVED = 0, NONE, NON_ODNS, UNKNOWN, NOT_MINE, MINE };

    #endregion

    #region Defines

    // Intervals upon which we fire threads
    public const int PROXY_CONNECT_RETRY_TIME = 2; // Short when can't connect
    public const int PROXY_STEADY_STATE_TIME = 10; // Slightly longer when we're doing our periodic check
    public int PROXY_INTERVAL = PROXY_CONNECT_RETRY_TIME; // Time to update EDNS status (not const as it can change)
    public const int NIC_INTERVAL = 1; // Time to update NICs
    public const int SERVICE_INTERVAL = 1; // Time to check service still running

    public static string g_sProxyIP = "127.0.0.1";
    //public static string g_sResolverIP = "67.215.69.213"; // bld3.dev.opendns.com
    public static string g_sResolverIP = "208.67.222.222"; // opendns.com
    //public static string g_sIDIP = "device-id.opendns.com";
    public static string g_sIDIP = "get.a.id.opendns.com";

    public static string DNSCRYPT_PROC_NAME = @"dnscrypt-proxy.exe";
    public static string DNSCRYPT_SVC_NAME = "DNSCrypt";

    #endregion

    #region Members

    // Timer members
    int m_nTimeToFire = 0;
    System.Timers.Timer m_MainTimer = null;

    // State members
    StateManager m_CurState = null;
    int m_nProxyTimer = 0; // For use in not firing on every elapsed event
    int m_nNICTimer = 0; // For use in updating NICs
    int m_nServiceTimer = 0; // For use in checking service running

    // Logging
    Logging m_Log;
    const string sLogPrefix = "OpenDnsService";
    //string sLogPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    
    //PORT_STATE m_LastPortState = PORT_STATE.RESERVED;
    //NETWORK_STATE m_LastNetworkState = NETWORK_STATE.RESERVED;

    // Threads
    Thread m_NICThread = null;
    Thread m_ProxyThread = null;
    Thread m_ServiceThread = null;
    Thread m_ExceptionThread = null;

    #endregion

    #region Classes

    #region State Manager

    // class to represent the current state of all the
    // necessary odds and ends of the client. Locked on access and update
    // at individual member level. This keeps us from locking the whole
    // object if only one thing is being read/updated (where safe)
    private class StateManager
    {
        static int MAX_FAIL = 2;
        public int m_nNetworkFailCount = 0;

        public bool m_bShutdown = false;

        public ServiceManager m_ServiceManager = null;

        #region User Desired Network State

        private bool m_bUseDNSCrypt = false;
        private bool m_bUseDNSCryptTCP = false;
        private bool m_bUseOpenDNS = false;
        private bool m_bUseInsecure = false;

        #endregion

        #region Actual Network State

        // State of our configuration
        private bool m_bNICsSet = false;
        private NICHandler.IP_CHOICES m_NICsWhere = NICHandler.IP_CHOICES.DEFAULT;
        private bool m_bProxyRunning = false;

        // State of network discovery
        private bool m_bGotResponse = false;
        private bool m_bCanContactODNS = false;
        private bool m_bAltPort = false;
        private bool m_bCanUseEncrypted = false;

        private int m_nProxyFailCount = 0;
        private RUN_STATE m_RunState = RUN_STATE.RESERVED;

        // State of IP
        private byte[] m_bIPv4 = new byte[] { 0x00, 0x00, 0x00, 0x00 };
        private byte[] m_bIPv6 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        #endregion

        public StateManager()
        {
            m_ServiceManager = new ServiceManager(DNSCRYPT_SVC_NAME);
        }

        #region Access Functions

        #region Desired Network State Access

        public void SetDesiredNetworkState(bool bUseDNSCrypt, bool bUseDNSCryptTCP, bool bUseOpenDNS, bool bUseInsecure)
        {
            m_bUseDNSCrypt = bUseDNSCrypt;
            m_bUseDNSCryptTCP = bUseDNSCryptTCP;
            m_bUseOpenDNS = bUseOpenDNS;
            m_bUseInsecure = bUseInsecure;
        }

        public void GetCanDoDesiredNetworkState(ref bool bUseDNSCrypt, ref bool bUseDNSCryptTCP, ref bool bUseOpenDNS, ref bool bUseInsecure)
        {
            bUseDNSCrypt = m_bUseDNSCrypt;
            bUseDNSCryptTCP = m_bUseDNSCryptTCP;
            bUseOpenDNS = m_bUseOpenDNS;
            bUseInsecure = m_bUseInsecure;
        }

        #endregion

        #region Actual Network State Access

        public void SetNetworkState(bool bGotResponse, bool bODNSAvailable)
        {
            lock (this)
            {
                if (!bGotResponse)
                {
                    m_nNetworkFailCount++;
                    if (m_nNetworkFailCount > MAX_FAIL)
                    {
                        m_bGotResponse = false;
                    }
                }
                else
                {
                    m_nNetworkFailCount = 0;
                    m_bGotResponse = true;
                }

                m_bCanContactODNS = bODNSAvailable;
            }
        }

        public void GetNetworkState(ref bool bGotResponse, ref bool bODNSAvailable, ref bool bAltPort, ref bool bIsProxyRunning, ref bool bCanUseEncrypted)
        {
            lock (this)
            {
                bGotResponse = m_bGotResponse;
                bODNSAvailable = m_bCanContactODNS;
                bAltPort = m_bAltPort;
                bIsProxyRunning = m_bProxyRunning;
                bCanUseEncrypted = m_bCanUseEncrypted;
            }
        }

        public bool IsOnAltPort()
        {
            bool bOut = false;
            lock (this)
            {
                bOut = m_bAltPort;
            }

            return bOut;
        }

        public bool GetHaveNetwork()
        {
            bool bOut = false;
            lock (this)
            {
                bOut = m_bGotResponse;
            }

            return bOut;
        }

        public void SetEncryptedState(bool bCanUseEncrypted)
        {
            lock (this)
            {
                m_bCanUseEncrypted = bCanUseEncrypted;
            }
        }

        #endregion

        #region NICs Access

        public void SetNICsState(NICHandler.IP_CHOICES Where, bool bNICSet)
        {
            m_bNICsSet = bNICSet;
            m_NICsWhere = Where;
        }

        public bool GetNICsSet(ref NICHandler.IP_CHOICES Where)
        {
            Where = m_NICsWhere;
            return m_bNICsSet;
        }

        #endregion

        #region Run State Access

        public void SetRunState(RUN_STATE State)
        {
            lock (this)
            {
                m_RunState = State;
            }
        }

        public RUN_STATE GetRunState()
        {
            RUN_STATE State = RUN_STATE.RESERVED;

            lock (this)
            {
                State = m_RunState;
            }

            return State;
        }

        #endregion

        #region Proxy Access

        public bool GetIsProxyRunning()
        {
            bool bOut = false;

            lock (this)
            {
                // If the service is found, and the network check
                // is ok AND the NICs are set, then proxy is good
                if (m_bProxyRunning && m_bNICsSet && (m_nProxyFailCount < 3))
                    bOut = true;
            }

            return bOut;
        }

        public void SetProxyRunning(bool bState)
        {
            m_bProxyRunning = bState;
        }

        #endregion

        #region DNSCrypt Access

        public bool GetDoDNSCrypt() 
        {
            bool bOut = false;

            lock (this)
            {
                if (m_bUseDNSCrypt)
                    bOut = true;
            }

            return bOut;
        }

        public bool GetDoDNSCryptTCP()
        {
            bool bOut = false;

            lock (this)
            {
                if (m_bUseDNSCryptTCP)
                    bOut = true;
            }

            return bOut;
        }

        public bool GetDoOpenDNSAvailable()
        {
            bool bOut = false;

            lock (this)
            {
                // If proxy is not running, the nics are set to OpenDNS,
                // we can ping OpenDNS, AND the user wants it, we are in
                // OpenDNS only
                if (m_bUseOpenDNS)
                    bOut = true;
            }

            return bOut;
        }

        #endregion

        #endregion
    }

    #endregion

    #endregion

    public TimedEventManager(int nElapsedTime, Logging Log)
    {
        m_nTimeToFire = nElapsedTime;

        m_Log = Log;

        m_Log.Log(Logging.LOGTYPE.DEBUG, "Attempting to create state manager");
        
        // We need a state manager to run
        m_CurState = new StateManager();
        
        m_Log.Log(Logging.LOGTYPE.DEBUG, "Successfully created state manager");
        
    }

    // Called when you want to start handling Events
    public void Begin()
    {
        DelayStart();

        m_CurState.SetRunState(RUN_STATE.FAIL_CLOSED);

        // Set up to check state on timed interval
        CreateMainTimer();

    }

    // Called to stop the timer
    public void CleanUp()
    {
        // First 
        m_CurState.m_bShutdown = true;

        WriteToLog("Closing OpenDNS Service");

        // Kill timer first
        m_MainTimer.Stop();
        m_MainTimer = null;

        try
        {
            // Stop service
            m_CurState.m_ServiceManager.KillServiceProcess();

            // Clean up any running threads

            if (m_NICThread.IsAlive)
                m_NICThread.Abort();

            if (m_ProxyThread.IsAlive)
                m_ProxyThread.Abort();

            // Set NIC back to default so the user has some DNS
            NICHandler.SetDNSServer(NICHandler.IP_CHOICES.DEFAULT);
        }
        catch (Exception Ex)
        {
            string sMsg = Ex.Message;
        }
        finally
        {
        }
    }

    public bool GetHaveNetwork()
    {
        return m_CurState.GetHaveNetwork();
    }

    public bool GetIsStopping()
    {
        return m_CurState.m_bShutdown;
    }

    // Accessess the inner state manager function which is thread safe
    public bool IsOnAltPort()
    {
        if (m_CurState == null)
            return false;

        return m_CurState.IsOnAltPort();
    }

    public int GetLogLevel()
    {
        // TODO:
        return 0; // this.m_CurState.GetLogLevel();
    }

    // The tell us what state they want
    public void SetDesiredState(bool bUseDNSCrypt, bool bUseDNSCryptTCP, bool bUseOpenDNS, bool bUseInsecure)
    {
        m_CurState.SetDesiredNetworkState(bUseDNSCrypt, bUseDNSCryptTCP, bUseOpenDNS, bUseInsecure);
    }

    // We tell them what they get
    public TimedEventManager.RUN_STATE GetCurrentState()
    {
        return m_CurState.GetRunState();
    }

    // Creates and sets running based on member elapsed interval
    private void CreateMainTimer()
    {
        m_MainTimer = new System.Timers.Timer();
        m_MainTimer.AutoReset = true;
        m_MainTimer.Interval = m_nTimeToFire;
        m_MainTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_MainTimer_Elapsed);
        m_MainTimer.Enabled = true;
        m_MainTimer.Start();
    }

    // Keep from having all clients synced
    private void DelayStart()
    {
        try
        {
            // Read the clock speed of the processor and use it as a seed so
            // on windows update, when all the clients restart, they don't sync
            int nSeed = 0;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_Processor");
            ManagementObjectCollection col = searcher.Get();
            foreach (ManagementObject oCur in col)
            {
                foreach (PropertyData Data in oCur.Properties)
                {
                    string sCur = Data.Name as string;
                    if (sCur != null)
                        if (sCur == "CurrentClockSpeed")
                        {
                            nSeed = Convert.ToInt32(Data.Value);
                        }
                }
            }
            Random rand = new Random(nSeed);
            nSeed = rand.Next(100, 1000);

            WriteToLog("Delaying start by: " + Convert.ToString(nSeed) + " milliseconds");
            System.Threading.Thread.Sleep(nSeed);
        }
        catch (Exception Ex)
        {

        }
    }

    private void WriteToLog(string msg)
    {
        m_Log.Log(sLogPrefix, msg);
    }

    #region State Functions

    private void SetState()
    {
        NICHandler.IP_CHOICES Where = NICHandler.IP_CHOICES.DEFAULT;
        bool bNICsSet = m_CurState.GetNICsSet(ref Where);

        bool bGotNetwork = false;
        bool bODNSAvailable = false;
        bool bIsProxyRunning = false;
        bool bIsAltPort = false;
        bool bCanUseEncrypted = false;

        m_CurState.GetNetworkState(ref bGotNetwork, ref bODNSAvailable, ref bIsAltPort, ref bIsProxyRunning, ref bCanUseEncrypted);

        bool bUseDNSCrypt = false;
        bool bUseDNSCryptTCP = false;
        bool bUseOpenDNS = false;
        bool bUseInsecure = false;

        m_CurState.GetCanDoDesiredNetworkState(ref bUseDNSCrypt, ref bUseDNSCryptTCP, ref bUseOpenDNS, ref bUseInsecure);

        // If we're not using TCP, and we want to use DNSCrypt, we must make sure we're encrypted
        if (!bUseDNSCryptTCP && bUseDNSCrypt)
        {
            // No secure connection means FAIL OVER
            if (!bGotNetwork || !bCanUseEncrypted)
            {
                if (bUseInsecure)
                {
                    m_CurState.SetRunState(RUN_STATE.FAIL_OPEN);
                }
                else
                {
                    m_CurState.SetRunState(RUN_STATE.FAIL_CLOSED);
                }

                if (!bUseDNSCryptTCP)
                {
                    return;
                }
            }
        }

        switch (m_CurState.GetRunState())
        {
            case RUN_STATE.FAIL_OPEN:

                if (bUseDNSCrypt && !bUseDNSCryptTCP && bCanUseEncrypted)
                    m_CurState.SetRunState(RUN_STATE.DNSCRYPT_FULL);
                else if(bUseDNSCrypt && bUseDNSCryptTCP)
                    m_CurState.SetRunState(RUN_STATE.DNSCRYPT_TCP);
                else if (!bUseDNSCryptTCP && bUseOpenDNS)
                    m_CurState.SetRunState(RUN_STATE.OPENDNS_ONLY);

                break;

            case RUN_STATE.FAIL_CLOSED:

                if (bUseDNSCrypt && !bUseDNSCryptTCP)
                    m_CurState.SetRunState(RUN_STATE.DNSCRYPT_FULL);
                else if(bUseDNSCrypt && bUseDNSCryptTCP)
                    m_CurState.SetRunState(RUN_STATE.DNSCRYPT_TCP);
                else if (!bUseDNSCrypt && !bUseDNSCryptTCP && bUseOpenDNS)
                    m_CurState.SetRunState(RUN_STATE.OPENDNS_ONLY);

                break;

            case RUN_STATE.SERVICE_RUNNING:

                if (bUseDNSCrypt && bUseDNSCryptTCP && bIsProxyRunning && bNICsSet && Where == NICHandler.IP_CHOICES.LOCALHOST)
                    m_CurState.SetRunState(RUN_STATE.DNSCRYPT_TCP);
                else if (bUseDNSCrypt && bIsProxyRunning && bNICsSet && Where == NICHandler.IP_CHOICES.LOCALHOST)
                    m_CurState.SetRunState(RUN_STATE.DNSCRYPT_FULL);
                else if (!bUseDNSCrypt && !bUseDNSCryptTCP && bUseOpenDNS && bNICsSet && Where == NICHandler.IP_CHOICES.OPENDNS)
                    m_CurState.SetRunState(RUN_STATE.OPENDNS_ONLY);

                break;

            case RUN_STATE.SERVICE_RESTART:

                if (bUseDNSCrypt && bUseDNSCryptTCP && bIsProxyRunning && bNICsSet && Where == NICHandler.IP_CHOICES.LOCALHOST)
                    m_CurState.SetRunState(RUN_STATE.DNSCRYPT_TCP);
                else if (bUseDNSCrypt && !bUseDNSCryptTCP && bIsProxyRunning && bNICsSet && Where == NICHandler.IP_CHOICES.LOCALHOST)
                    m_CurState.SetRunState(RUN_STATE.DNSCRYPT_FULL);
                else if (!bUseDNSCrypt && !bUseDNSCryptTCP && bUseOpenDNS && bNICsSet && Where == NICHandler.IP_CHOICES.OPENDNS)
                    m_CurState.SetRunState(RUN_STATE.OPENDNS_ONLY);

                break;

            case RUN_STATE.DNSCRYPT_FULL:

                if (!bUseDNSCrypt && bIsProxyRunning && !bUseOpenDNS)
                    m_CurState.SetRunState(RUN_STATE.FAIL_OPEN);
                else if (bUseDNSCrypt && bUseDNSCryptTCP && bIsProxyRunning && bUseOpenDNS)
                    m_CurState.SetRunState(RUN_STATE.SERVICE_RESTART);
                else if (!bUseDNSCrypt && bUseOpenDNS)
                    m_CurState.SetRunState(RUN_STATE.OPENDNS_ONLY);
                
                break;

            case RUN_STATE.DNSCRYPT_TCP:

                if (!bUseDNSCrypt && bIsProxyRunning && !bUseOpenDNS)
                    m_CurState.SetRunState(RUN_STATE.FAIL_OPEN);
                else if (bUseDNSCrypt && !bUseDNSCryptTCP && bIsProxyRunning && bUseOpenDNS)
                    m_CurState.SetRunState(RUN_STATE.SERVICE_RESTART);
                else if (!bUseDNSCrypt && bUseOpenDNS)
                    m_CurState.SetRunState(RUN_STATE.OPENDNS_ONLY);

                break;

            case RUN_STATE.OPENDNS_ONLY:

                if (bUseDNSCrypt && !bUseDNSCryptTCP && bUseOpenDNS)
                    m_CurState.SetRunState(RUN_STATE.DNSCRYPT_FULL);
                if (bUseDNSCrypt && bUseDNSCryptTCP && bUseOpenDNS)
                    m_CurState.SetRunState(RUN_STATE.DNSCRYPT_TCP);
                else if (!bUseDNSCrypt && !bUseOpenDNS && bUseInsecure)
                    m_CurState.SetRunState(RUN_STATE.FAIL_OPEN);
                else if (!bUseDNSCrypt && !bUseOpenDNS && !bUseInsecure)
                    m_CurState.SetRunState(RUN_STATE.FAIL_CLOSED);

                break;
        }
    }

    // Top level execution function for time based events
    void m_MainTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (m_CurState.m_bShutdown)
            return;

        SetState();

        QueryService();
        QueryNICs();
        QueryNetwork();
       
    }

    #endregion

    #region Service Functions

    // Just checks if the DNSCrypt service is running, and forces the state
    // the user desires if not already in that state
    public void DoCheckServiceRunning(object oIn)
    {
       StateManager CurState = (StateManager)oIn;

        if (CurState.m_bShutdown)
            return;

        RUN_STATE State = CurState.GetRunState();
        switch (State)
        {
            case RUN_STATE.SERVICE_RUNNING:
            case RUN_STATE.DNSCRYPT_FULL:

                if (!CurState.m_ServiceManager.IsServiceRunning())
                {
                    // Not running, launch one
                    WriteToLog("Starting OpenDNS Service - " + CurState.m_ServiceManager.StartServiceProcess(CurState.GetDoDNSCryptTCP()));

                    CurState.SetProxyRunning(false);

                }
                else if (ProcessManager.ProcessExists(DNSCRYPT_PROC_NAME) > 0)
                {
                    // Everything is ok, the service exists
                    CurState.SetProxyRunning(true);
                }

                break;
            
            case RUN_STATE.DNSCRYPT_TCP:

                if (!CurState.m_ServiceManager.IsServiceRunningInTcpMode())
                {
                    if (!CurState.m_ServiceManager.IsServiceRunning())
                    {
                        // Not running, launch one
                        WriteToLog("Starting OpenDNS Service - " + CurState.m_ServiceManager.StartServiceProcess(CurState.GetDoDNSCryptTCP()));
                    }
                    else
                    {
                        // Already running, restart
                        CurState.m_ServiceManager.RestartServiceProcess(CurState.GetDoDNSCryptTCP());
                    }
                    
                    CurState.SetProxyRunning(false);

                }
                else if (ProcessManager.ProcessExists(DNSCRYPT_PROC_NAME) > 0)
                {
                    // Everything is ok, the service exists
                    CurState.SetProxyRunning(true);
                }

                break;

            case RUN_STATE.SERVICE_RESTART:

                CurState.m_ServiceManager.RestartServiceProcess(CurState.GetDoDNSCryptTCP());

                CurState.SetProxyRunning(true);

                break;

            case RUN_STATE.FAIL_OPEN:

                // Keep the proxy running but set NICs to default
                if (!CurState.m_ServiceManager.IsServiceRunning())
                {
                    // Not running, launch one
                    WriteToLog("Starting OpenDNS Service - " + CurState.m_ServiceManager.StartServiceProcess(CurState.GetDoDNSCryptTCP()));
                }

                break;

            case RUN_STATE.FAIL_CLOSED:

                // Keep the proxy running and NICs pointed at it
                if (!CurState.m_ServiceManager.IsServiceRunning())
                {
                    // Not running, launch one
                    WriteToLog("Starting OpenDNS Service - " + CurState.m_ServiceManager.StartServiceProcess(CurState.GetDoDNSCryptTCP()));
                }

                break;

            default:

                // All we care about in other states is kill any processes hanging
                // around if we don't need them
                if (CurState.m_ServiceManager.IsServiceRunning())
                {
                    CurState.m_ServiceManager.KillServiceProcess();

                    CurState.SetProxyRunning(false);
                }

                break;
        }
    }

    #endregion

    #region NIC Functions

    // Thread function to check NIC settings and force if
    // not right
    private void MonitorNICs(object oIn)
    {
        StateManager CurState = (StateManager)oIn;

        if (CurState.m_bShutdown)
            return;

        RUN_STATE Now = CurState.GetRunState();

        if ((Now == RUN_STATE.DNSCRYPT_FULL) || (Now == RUN_STATE.DNSCRYPT_TCP) || (Now == RUN_STATE.SERVICE_RESTART))
        {
            // Keep monitoring they are set to proxy
            CurState.SetNICsState(NICHandler.IP_CHOICES.LOCALHOST, NICHandler.EnsureNICsState(NICHandler.IP_CHOICES.LOCALHOST));
        }
        else if (Now == RUN_STATE.SERVICE_RUNNING)
        {
            // Set the NICs to proxy
            CurState.SetNICsState(NICHandler.IP_CHOICES.LOCALHOST, NICHandler.EnsureNICsState(NICHandler.IP_CHOICES.LOCALHOST));
        }
        else if (Now == RUN_STATE.OPENDNS_ONLY)
        {
            // Set the NICs to OpenDNS
            CurState.SetNICsState(NICHandler.IP_CHOICES.OPENDNS, NICHandler.EnsureNICsState(NICHandler.IP_CHOICES.OPENDNS));
        }
        else if (Now == RUN_STATE.FAIL_CLOSED)
        {
            // Set NICs to Proxy
            CurState.SetNICsState(NICHandler.IP_CHOICES.LOCALHOST, NICHandler.EnsureNICsState(NICHandler.IP_CHOICES.LOCALHOST));
        }
        else if (Now == RUN_STATE.FAIL_OPEN)
        {
            // Set the NICs to OpenDNS
            CurState.SetNICsState(NICHandler.IP_CHOICES.OPENDNS, NICHandler.EnsureNICsState(NICHandler.IP_CHOICES.OPENDNS));
        }
        else
        {
            // Set or ensure NICs are original user settings
            CurState.SetNICsState(NICHandler.IP_CHOICES.DEFAULT, NICHandler.EnsureNICsState(NICHandler.IP_CHOICES.DEFAULT));
        }

    }

    #endregion

    #region Proxy Functions

    // Actually checks to see if data can go to the internet (via UDP 53)
    private void DoCheckDNSNetwork(object oIn)
    {
        try
        {
            StateManager CurState = (StateManager)oIn;

            byte[] bIDOut = new byte[8];

            // First, try a DNS ping through proxy (whether it's on or not)
            EDNSPacket Packet = SendNetworkPacket(g_sProxyIP, g_sIDIP, 53, 0x10, false, bIDOut);

            if (Packet.GetResponsePacket().Length > 0)
            {
                // We got a result through the proxy; all is well
                CurState.SetEncryptedState(true);
                CurState.SetNetworkState(true, true);
            }
            else
            {
                // We did not get a response through the proxy, so encryption is a no-go
                CurState.SetEncryptedState(false);

                // Try a second DNS ping, this time through the OpenDNS resolver
                Packet = SendNetworkPacket(g_sResolverIP, g_sIDIP, 53, 0x10, false, bIDOut);
                if (Packet.GetResponsePacket().Length > 0)
                {
                    // We have at least SOME internet connection
                    CurState.SetNetworkState(true, true);
                }
                else
                {
                    // We could not reach the network at all
                    CurState.SetNetworkState(false, true);
                }
            }
            
        }
        catch (Exception Ex)
        {
            // Do nothing
        }
    }

    #endregion

    #region Threaded State Functions

    // NOTE: These functions should all launch threads as they could take
    // enough processing time that they need to be decoupled

    private void QueryService()
    {
        // Check on interval
        if (m_nServiceTimer % SERVICE_INTERVAL == 0)
        {
            // Relaunch the nic check thread if not alive
            if ((m_ServiceThread == null) || (!m_ServiceThread.IsAlive))
            {
                m_ServiceThread = new Thread(new ParameterizedThreadStart(DoCheckServiceRunning));
                m_ServiceThread.Start(m_CurState);

                m_nServiceTimer = 0;
            }
        }

        m_nServiceTimer++;
    }

    private void QueryNetwork()
    {
        // Check on interval
        if (m_nProxyTimer % PROXY_INTERVAL == 0)
        {
            // Relaunch the nic check thread if not alive
            if ((m_ProxyThread == null) || (!m_ProxyThread.IsAlive))
            {
                m_ProxyThread = new Thread(new ParameterizedThreadStart(DoCheckDNSNetwork));
                m_ProxyThread.Start(m_CurState);

                m_nProxyTimer = 0;
            }
        }

        m_nProxyTimer++;
    }

    private void QueryNICs()
    {
        if (m_CurState.m_bShutdown)
            return;

        if (m_nNICTimer % NIC_INTERVAL == 0)
        {

            // Relaunch the nic check thread if not alive
            if ((m_NICThread == null) || (!m_NICThread.IsAlive))
            {
                m_NICThread = new Thread(new ParameterizedThreadStart(MonitorNICs));
                m_NICThread.Start(m_CurState);
            }
        }

        m_nNICTimer++;
    }

    #endregion

    // Actually builds, sends, sets the received bytes and returns the whole packet
    private EDNSPacket SendNetworkPacket(string sDNSIP, string sIPToResolve, int nPort, byte bType, bool bEDNS, byte[] bMachine_ID)
    {
        // Create empty EDNS packet
        EDNSPacket Packet = new EDNSPacket();

        try
        {
            IPEndPoint Endpoint = new IPEndPoint(IPAddress.Parse(sDNSIP), nPort);

            // Send the current machine_id host
            if (bEDNS)
                Packet.CreateEDNSPacketMachineID(sIPToResolve, bType, bMachine_ID);
            else
                Packet.CreateDNSPacket(sIPToResolve, bType);

            if (Packet.GetPacketLen() > 0)
            {
                // Create a udp client to send the packet
                UdpClient udpGo = new UdpClient();
                udpGo.Client.SendTimeout = 2000;
                udpGo.Client.ReceiveTimeout = 2000;
                udpGo.Send(Packet.GetPacket(), Packet.GetPacket().Length, Endpoint);

                //IPEndPoint object will allow us to read datagrams sent from any source.
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                // Launch Asynchronous
                IAsyncResult iarResult = udpGo.BeginReceive(null, null);

                // Wait until complete
                bool bKill = false;
                DateTime dtTimeStart = DateTime.Now;
                while (iarResult.IsCompleted == false)
                {
                    // Watchdog, if doesn't return in 5 seconds get out
                    if (dtTimeStart.AddSeconds(5) < DateTime.Now)
                    {
                        bKill = true;
                        break;
                    }
                }

                // This can hang when not happy about a broken connection
                if (bKill)
                    udpGo.Close();
                else
                    Packet.SetReceivePacket(udpGo.EndReceive(iarResult, ref RemoteIpEndPoint));
            }

        }
        catch (Exception Ex)
        {
            // TODO: Log an exception?
        }

        // Always just return packet
        return Packet;
    }
}
