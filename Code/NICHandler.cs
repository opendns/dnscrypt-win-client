using System;
using System.Collections.Generic;

// Nic access
using System.Management;
using System.Net.NetworkInformation;
using System.Net;

// Registry
using Microsoft.Win32;

// Shell Execute
using System.Diagnostics;


// Handles all associated stuff for setting/getting NIC info 
// and DNS settings
public static class NICHandler
{
    // One copy
    static List<NICObject> m_NICPersisteSettings = null;

    public enum IP_CHOICES { DEFAULT, LOCALHOST, OPENDNS, FORCE_AUTO };

    #region Classes

    private class NICObject
    {
        public NetworkInterface m_NIC = null;
        public bool m_bIsObtainAuto = false;

        public NICObject(NetworkInterface NIC, bool bIsAuto)
        {
            m_NIC = NIC;
            m_bIsObtainAuto = bIsAuto;
        }
    }

    #endregion 

    #region Public Functions

    // Sets every NIC possible to point to the DNS server of our choice
    public static bool SetDNSServer(IP_CHOICES Choice)
    {
        try
        {
            SetNICs(Choice);
        }
        catch (Exception Ex)
        {
            string sMsg = Ex.Message;
            return false;
        }

        return true;
    }

    // Handles the whole task of making sure the NICs are still
    // set to the correct settings once they have been initially set
    public static bool EnsureNICsState(IP_CHOICES Choice)
    {
        try
        {
            // Handles first run and all that
            SaveCurNICSettings();

            List<NetworkInterface> ResetNICs = TestNICs(Choice);

            // Reset the offenders
            foreach (NetworkInterface NIC in ResetNICs)
            {
                switch (Choice)
                {
                    case IP_CHOICES.DEFAULT:
                        SetDNSDefault(NIC);
                        break;
                    case IP_CHOICES.LOCALHOST:
                        SetDNSProxy(NIC);
                        break;
                    case IP_CHOICES.OPENDNS:
                        SetDNSOpenDNS(NIC);
                        break;
                }                    
            }
        }
        catch (Exception Ex)
        {
            string sMsg = Ex.Message;
            return false;
        }

        return true;
    }

    // Gets the initial settings for the NICs so we can set them back
    // on shutdown
    public static void SaveCurNICSettings()
    {
        // Should be null if never run. Should only be run once.
        if (m_NICPersisteSettings == null)
        {
            m_NICPersisteSettings = new List<NICObject>();
            NetworkInterface[] NICs = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface NIC in NICs)
            {
                NICObject CurNIC = new NICObject(NIC, IsObtainDNSAuto(NIC.Id));
                m_NICPersisteSettings.Add(CurNIC);
            }
        }
    }

    #endregion

    #region Private Functions

    static void SetNICs(IP_CHOICES Choice)
    {
        NetworkInterface[] NICs = NetworkInterface.GetAllNetworkInterfaces();
        foreach (NetworkInterface NIC in NICs)
        {
            // Only full NICs we care about
            if (!IsReceiveOnly(NIC))
            {
                IPInterfaceProperties IPProps = NIC.GetIPProperties();
                IPAddressCollection ipCollDNS = IPProps.DnsAddresses;

                foreach (IPAddress Addr in ipCollDNS)
                {
                    switch (Choice)
                    {
                        case IP_CHOICES.DEFAULT:
                            SetDNSDefault(NIC);
                            break;

                        case IP_CHOICES.LOCALHOST:
                            SetDNSProxy(NIC);
                            break;

                        case IP_CHOICES.OPENDNS:
                            SetDNSOpenDNS(NIC);
                            break;

                        case IP_CHOICES.FORCE_AUTO:
                            ForceDNSDefault(NIC);
                            break;
                    }
                }
                    
            }
        }
    }

    // Tests all NICs for current DNS settings, and returns a list not matching
    // the input state
    static List<NetworkInterface> TestNICs(IP_CHOICES Choice)
    {
        List<NetworkInterface> ResetNICs = new List<NetworkInterface>();
        NetworkInterface[] NICs = NetworkInterface.GetAllNetworkInterfaces();
        foreach (NetworkInterface NIC in NICs)
        {
            // Only full NICs we care about
            if (!IsReceiveOnly(NIC))
            {
                IPInterfaceProperties IPProps = NIC.GetIPProperties();
                IPAddressCollection ipCollDNS = IPProps.DnsAddresses;

                if (NIC.OperationalStatus != OperationalStatus.Up)
                    continue;

                foreach (IPAddress Addr in ipCollDNS)
                {
                    if (Addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                        continue;

                    switch (Choice)
                    {
                        case IP_CHOICES.DEFAULT:

                            if (Addr.ToString() != "")
                            {
                                ResetNICs.Add(NIC);
                            }
                            break;

                        case IP_CHOICES.LOCALHOST:

                            if (Addr.ToString() != "127.0.0.1")
                            {
                                ResetNICs.Add(NIC);
                            }

                            break;

                        case IP_CHOICES.OPENDNS:

                            if ((Addr.ToString() != "208.67.220.220") && (Addr.ToString() != "208.67.222.222"))
                            {
                                ResetNICs.Add(NIC);
                            }

                            break;
                    }
                }

            }
        }

        return ResetNICs;
    }

    #region WMI Versions

    static void SetDNSProxy(NetworkInterface NIC)
    {
        // Generated by the MS WMI tool
        try
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                if ((bool)mo["IPEnabled"])
                {
                    ManagementBaseObject objdns = mo.GetMethodParameters("SetDNSServerSearchOrder");

                    if (objdns != null)
                    {
                        string[] s = { "127.0.0.1" };
                        objdns["DNSServerSearchOrder"] = s;
                        mo.InvokeMethod("SetDNSServerSearchOrder", objdns, null);
                    }

                }

            }
        }
        catch (ManagementException e)
        {
        }
    }

    static void SetDNSOpenDNS(NetworkInterface NIC)
    {
        // Generated by the MS WMI tool
        try
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                if ((bool)mo["IPEnabled"])
                {
                    ManagementBaseObject objdns = mo.GetMethodParameters("SetDNSServerSearchOrder");

                    if (objdns != null)
                    {
                        string[] sDNSSearchList = new string[2];
                        sDNSSearchList[0] = "208.67.220.220";
                        sDNSSearchList[1] = "208.67.222.222";

                        objdns["DNSServerSearchOrder"] = sDNSSearchList;
                        mo.InvokeMethod("SetDNSServerSearchOrder", objdns, null);
                    }

                }

            }
        }
        catch (ManagementException e)
        {
        }
    }

    static void SetDNSDefault(NetworkInterface NIC)
    {
        // Get the state this guy was originally in
        NICObject UseNIC = null;
        foreach (NICObject OrigNIC in m_NICPersisteSettings)
        {
            if (OrigNIC.m_NIC.Id == NIC.Id)
                UseNIC = OrigNIC;
        }

        // Gets the adapter config and search to find DNS settings portion
        try
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                // Only look for this NIC (by ID)
                string sCurID = (string)mo["SettingID"];
                if ((sCurID == UseNIC.m_NIC.Id))
                {
                    // Get the DNS search order property
                    ManagementBaseObject objdns = mo.GetMethodParameters("SetDNSServerSearchOrder");
                    if (objdns != null)
                    {
                        // If we found the NIC in our saved ones above (should have)
                        // then use its original settings
                        if (UseNIC != null)
                        {
                            // SET TO ORIGINAL SETTINGS

                            // The original settings could have been "obtain DNS server automatically"
                            if (UseNIC.m_bIsObtainAuto)
                            {
                                // SET TO "obtain DNS server automatically"

                                // Call this guy with no "in" params sets it to default
                                object oRet = mo.InvokeMethod("SetDNSServerSearchOrder", null);
                            }
                            else
                            {
                                // SET TO HARD ADDRESS

                                IPAddressCollection IPColl = UseNIC.m_NIC.GetIPProperties().DnsAddresses;
                                string[] sDNSSearchList = new string[IPColl.Count];
                                int i = 0;
                                foreach (IPAddress CurIP in IPColl)
                                {
                                    sDNSSearchList[i++] = CurIP.ToString();
                                }

                                objdns["DNSServerSearchOrder"] = sDNSSearchList;
                                mo.InvokeMethod("SetDNSServerSearchOrder", objdns, null);
                            }
                        }
                        else
                        {
                            // SET TO "obtain DNS server automatically"

                            // Call this guy with no "in" params sets it to default
                            object oRet = mo.InvokeMethod("SetDNSServerSearchOrder", null);
                        }
                    }

                }

            }
        }
        catch (ManagementException e)
        {
            string sStack = e.ToString();
        }
    }

    static void ForceDNSDefault(NetworkInterface NIC)
    {
        // Generated by the MS WMI tool
        try
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                if ((bool)mo["IPEnabled"])
                {
                    ManagementBaseObject objdns = mo.GetMethodParameters("SetDNSServerSearchOrder");

                    if (objdns != null)
                    {
                        // SET TO "obtain DNS server automatically"

                        // Call this guy with no "in" params sets it to default
                        object oRet = mo.InvokeMethod("SetDNSServerSearchOrder", null);
                    }

                }

            }
        }
        catch (ManagementException e)
        {
        }
    }

    #endregion

    #region Registry Key Functions

    // Gets the "NameServer" value under the NIC with the input ID
    static bool IsObtainDNSAuto(string NICId)
    {
        // Look up the registry key for network interface NICId
        RegistryKey RootKey = Registry.LocalMachine;

        string sNICKey = @"SYSTEM\CurrentControlSet\services\Tcpip\Parameters\Interfaces\";
        sNICKey += NICId;
        string sOut = GetRegKeyString(RootKey, sNICKey, "NameServer");
        if (sOut.Length == 0)
        {
            return true;
        }
        else
        {
            // 127.0.0.1 is the same as default from our perspective.
            // This is because we need to make sure we don't fail to
            // set back on a previous run and then hang ourselves
            if (sOut == "127.0.0.1")
                return true;

            return false;
        }
    }

    #endregion

    #region Shell Execute Functions

    // Runs arbitrary shell command program with input args
    static bool RunShellCommand(ref Process CurProc, string sProgram, string sCommandLine)
    {
        Console.WriteLine("Running: " + sCommandLine);
        try
        {
            if (CurProc == null)
            {
                // Create a new one
                ProcessStartInfo ProcInfo = new ProcessStartInfo(sProgram, sCommandLine);
                ProcInfo.CreateNoWindow = true;
                ProcInfo.UseShellExecute = false;
                ProcInfo.RedirectStandardInput = true;
                ProcInfo.RedirectStandardOutput = true;
                //ProcInfo.Arguments = sCommandLine; // Doesn't work for some reason
                CurProc = new Process();
                CurProc.StartInfo = ProcInfo;
                if (!CurProc.Start())
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        catch (Exception Ex)
        {
            // Possibly don't have permissions
            Console.WriteLine(Ex.Message);
            return false;
        }

        // Just return
        return true;
    }

    #endregion

    #region Utility

    static bool SetRegKey(RegistryKey RootKey, string sPath, string sKey, string sValue)
    {
        try
        {
            RegistryKey CurKey = RootKey.OpenSubKey(sPath);
            if (CurKey != null)
            {
                // Actually set it
                CurKey.SetValue(sKey, sValue);
            }
        }
        catch (Exception Ex)
        {
            Console.WriteLine(Ex.Message);
            return false;
        }

        return true;
    }

    static string GetRegKeyString(RegistryKey RootKey, string sPath, string sKey)
    {
        string sOut = "";

        try
        {
            RegistryKey CurKey = RootKey.OpenSubKey(sPath);
            if (CurKey != null)
            {
                // Actually set it
                RegistryValueKind vkCur = CurKey.GetValueKind(sKey);
                if (vkCur == RegistryValueKind.String)
                {
                    sOut = (string)CurKey.GetValue(sKey);
                }
                // else return empty
            }
        }
        catch (Exception Ex)
        {
            string sMsg = Ex.Message;
        }

        return sOut;
    }

    static bool IsReceiveOnly(NetworkInterface NIC)
    {
        bool bRxOnly = true; 
        try 
        { 
            bRxOnly = NIC.IsReceiveOnly; 
        }
        catch (Exception) 
        {
            // Do nothing
        }

        return bRxOnly;
    }

    #endregion

    #endregion

}
