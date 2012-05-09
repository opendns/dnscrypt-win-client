using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;

using System.ServiceProcess;
using System.Text;

using System.Threading;
using System.Timers;

namespace OpenDNSCryptService
{
    public partial class Service1 : ServiceBase
    {
        public enum START_MODE { USE_NORMAL = 129, USE_TCP = 130 };
        bool m_bStopping = false;
        string m_sParams = "";
        System.Timers.Timer m_MainTimer = null;

        public static string DNSCRYPT_PROC_NAME = @"dnscrypt-proxy.exe";

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (args.Length > 0)
            {
                Properties.Settings.Default.StartMode = args[0];
                Properties.Settings.Default.Save();
            }

            // Initially launch proxy
            ProcessManager.LaunchProcess(DNSCRYPT_PROC_NAME, Properties.Settings.Default.StartMode);

            // Launch the timer
            StartTimer();
        }

        protected override void OnStop()
        {
            m_bStopping = true;

            m_MainTimer.Stop();
            m_MainTimer = null;

            ProcessManager.KillExistingProcesses(DNSCRYPT_PROC_NAME);
        }

        protected override void OnCustomCommand(int command)
        {
            // If either of these, just kill the running proxy,
            // on timer the process will restart
            if (command == (int)START_MODE.USE_TCP)
            {
                ProcessManager.KillExistingProcesses(DNSCRYPT_PROC_NAME);
                Properties.Settings.Default.StartMode = "--tcp-port=443";
                Properties.Settings.Default.Save();
            }
            else if (command == (int)START_MODE.USE_NORMAL)
            {
                ProcessManager.KillExistingProcesses(DNSCRYPT_PROC_NAME);
                Properties.Settings.Default.StartMode = "";
                Properties.Settings.Default.Save();
            }

            base.OnCustomCommand(command);
        }

        // Mainly here to watchdog the proxy
        void m_MainTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!m_bStopping)
            {
                if (ProcessManager.ProcessExists(DNSCRYPT_PROC_NAME) == 0)
                {
                    ProcessManager.LaunchProcess(DNSCRYPT_PROC_NAME, m_sParams);
                }
            }
        }

        #region Utility

        private void StartTimer()
        {
            m_MainTimer = new System.Timers.Timer();
            m_MainTimer.AutoReset = true;
            m_MainTimer.Interval = 2000;
            m_MainTimer.Elapsed += new ElapsedEventHandler(m_MainTimer_Elapsed);
            m_MainTimer.Enabled = true;
            m_MainTimer.Start();
        }

        #endregion
    }
}
