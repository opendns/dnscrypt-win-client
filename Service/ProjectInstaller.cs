using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Management;
using System.IO;

using System.ServiceProcess;
using System.Diagnostics;
using System.Threading;
using System.Security.Principal;

namespace OpenDNSCryptService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        const string CLIENT_PROC_NAME = "OpenDNSInterface";

        public ProjectInstaller()
        {
            InitializeComponent();
        }

        protected override void OnBeforeInstall(IDictionary savedState)
        {
            //StreamWriter sw = File.CreateText("C:\\temp\\test.txt");
            //sw.Write("am admin: " + IsElevated());
            //sw.Close();

            base.OnBeforeInstall(savedState);
        }

        public override void Install(IDictionary stateSaver)
        {
            // Close running application
            foreach (Process p in Process.GetProcessesByName(CLIENT_PROC_NAME))
            {
                p.Kill();
            }

            // Kill service if running
            ProcessManager.LaunchProcess("sc", "stop DNSCrypt");

            base.Install(stateSaver);
        }

        public override void Uninstall(IDictionary savedState)
        {
            // Close running application
            foreach (Process p in Process.GetProcessesByName(CLIENT_PROC_NAME))
            {
                p.Kill();
            }

            // Kill service if running
            ProcessManager.LaunchProcess("sc", "stop DNSCrypt");

            // Set NICs back to auto discover
            NICHandler.SetDNSServer(NICHandler.IP_CHOICES.FORCE_AUTO);

            base.Uninstall(savedState);
        }

        private bool IsElevated()
        {
            AppDomain myDomain = Thread.GetDomain();
            myDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
            WindowsPrincipal myPrincipal = (WindowsPrincipal)Thread.CurrentPrincipal;
            return (myPrincipal.IsInRole(WindowsBuiltInRole.Administrator));
        }
    }
}
