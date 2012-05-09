using System;
using System.Collections.Generic;
using System.Windows.Forms;

using System.Diagnostics;

using System.Security.Principal;

using System.IO;

namespace OpenDNSInterface
{
    static class Program
    {
        static string sLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "OpenDNS");
        const string sLogFileName = "OpenDNSCrypt_Service";
        const string sLogPrefix = "OpenDnsService";
        static Logging m_Log = null;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Make sure our log directory exists
            if (!Directory.Exists(sLogPath)) Directory.CreateDirectory(sLogPath);

            // Create our Logger
            m_Log = new Logging(sLogFileName, sLogPath, true);
            m_Log.SetLogging(Logging.LOGTYPE.DEBUG);

            m_Log.Log(Logging.LOGTYPE.DEBUG, "In Main()");

            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            m_Log.Log(Logging.LOGTYPE.DEBUG, "Aquire Mutex");

            bool bOk = false;
            System.Threading.Mutex mSingle = new System.Threading.Mutex(true, "OpenDNSInterface", out bOk);

            if (!bOk)
            {
                m_Log.Log(Logging.LOGTYPE.DEBUG, "Attempting to kill other instances");

                // NOTE: Used to kill this instance, now kill existing instance. This allows upgrade
                // to be smoother so latest is running if it is started on top of old one...
                //MessageBox.Show("Another instance of OpenDNS DNSCrypt Client is already running.");

                // Close running application
                foreach (Process p in Process.GetProcessesByName("OpenDNSInterface"))
                {
                    p.Kill();
                }

                //return;
            }

            m_Log.Log(Logging.LOGTYPE.DEBUG, "App Run()");

            Application.Run(new OpenDNSInterface(m_Log));

            // Makes sure the program doesn't prematurely garbage collect the mutex
            // resulting in the ability to run the exe again...
            GC.KeepAlive(mSingle);
        }

        /// <summary>
        /// Handles any exceptions that occur in child threads.
        /// By doing this, we don't get annoying JIT popups that squash our debugging efforts.
        /// </summary>
        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            try
            {
                // Write out to the Debug trace if it's available (to save time)
                System.Diagnostics.Debug.WriteLine(e.Exception.ToString());

                // Create a MiniDump in the Dumps directory (also makes a WEP entry)
                CrashHandler.CreateMiniDump();

                if (m_Log != null)
                {
                    m_Log.Log(Logging.LOGTYPE.ERROR, "Checking privs in crash handler");
                }

                // We could now pop up a custom dialog if we desired
                if (IsAdmin())
                {
                    MessageBox.Show("OpenDNSCrypt has encountered an error and will now close.");
                }
                else
                {
                    MessageBox.Show("OpenDNSCrypt cannot continue because it requires administratrive priveleges to run. Please run as administrator.");
                }

                if (m_Log != null)
                {
                    m_Log.Log(Logging.LOGTYPE.ERROR, "Crash Exception: ");
                    m_Log.Log(Logging.LOGTYPE.ERROR, e.Exception.ToString());
                }

                // TODO: Throwing a new exception here gives us more info in our reports... investigate
                //throw new Exception("AHA");
            }
            finally
            {
                // We always want to exit after a crash
                Application.Exit();
            }
        }

        static bool IsAdmin()
        {
            WindowsIdentity wi = WindowsIdentity.GetCurrent();
            WindowsPrincipal wp = new WindowsPrincipal(wi);

            return wp.IsInRole(WindowsBuiltInRole.Administrator);

        }
    }
}
