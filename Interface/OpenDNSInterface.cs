using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Threading;
using System.Diagnostics;

using System.Reflection;

namespace OpenDNSInterface
{
    public partial class OpenDNSInterface : Form
    {
        #region Members

        TimedEventManager m_TimedEventManager = null;

        #region Enums

        enum PANELS { GENERAL, ABOUT, RELEASE_NOTES, PREVIEW_FEEDBACK };
        enum OVERALL_STATUS { OK, WARNING, BAD };
        enum OVERALL_STATE { NONE, ODNS_ONLY, DNS_CRYPT_FULL, DNSCRYP_TCP, DNS_INSECURE };

        #endregion

        #region Program Members

        System.Windows.Forms.Timer m_MainTimer = null;

        private bool m_bUseOpenDNS = false;
        private bool m_bUseDNSCrypt = false;
        private bool m_bUseDNSCryptTCP = false;
        private bool m_bUseInsecure = false;

        private OVERALL_STATE m_CurrentState = OVERALL_STATE.NONE;

        #endregion

        #region UI Members

        bool m_bExit = false; // Handle show/hide when user hits 'X' on window control box

        PANELS m_CurVisiblePanel = PANELS.GENERAL;

        string g_HOMEURL = "http://dnscrypt.opendns.com/";
        string g_GITURL = "http://opendns.github.com/dnscrypt-win-client/";

        #endregion

        #region Controls

        List<Control> m_Panels = new List<Control>();

        StatusPanel m_StatusPanel = null;
        HTMLPanel m_AboutPanel = null;
        HTMLPanel m_ReleasePanel = null;
        HTMLPanel m_FeedbackPanel = null;

        #endregion

        #region Resources

        Icon m_OkIcon = null;
        Icon m_ErrorIcon = null;
        Icon m_WarningIcon = null;

        //Bitmap m_Loading = null;

        #endregion

        #endregion

        #region Update Members

        System.Windows.Forms.Timer m_UpgradeTimer = null;
        string m_CurrentVersion = VersionInfo.UpdateVersion;
        Updater m_Updater = null;
        bool m_bUpgradeAvailable = false;
        bool m_bUpgradeServerFailed = false;
        bool m_bDownloadSuccess = false;

        Logging m_Log = null;

        #endregion

        public OpenDNSInterface(Logging Log)
        {
            m_Log = Log;


            m_Updater = new Updater(m_CurrentVersion);

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            InitializeComponent();
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Write crash dump before anything else
            CrashHandler.CreateMiniDump();
        }

        // Called before window is shown
        private void OpenDNSInterface_Load(object sender, EventArgs e)
        {
            // Does all setup procedures
            InitProgram();
        }

        // Override to minimize on X'ing out
        protected override void OnClosing(CancelEventArgs e)
        {
            if (this.Visible && !m_bExit)
            {
                // They used the 'X' on the window, which means they actually want to minimize
                e.Cancel = true;
                SetWindowVisible(false);
            }
            else if (m_bExit)
            {
                // Only stop the service if we've been explicitly told to exit (ie. via the menu)
                m_TimedEventManager.CleanUp();
            }

            // Call base either way
            base.OnClosing(e);
        }

        // Call on first load to set up custom components
        private void InitProgram()
        {
            m_Log.Log(Logging.LOGTYPE.ERROR, "Enter Init program()");

            // Hide it on startup
            SetWindowVisible(false);

            m_Log.Log(Logging.LOGTYPE.ERROR, "Load panels");
            LoadPanels();
            LoadIcons();

            m_Log.Log(Logging.LOGTYPE.ERROR, "Reading configured start");
            ReadConfiguredStart();

            m_Log.Log(Logging.LOGTYPE.ERROR, "Set up stats panel");
            SetStatusPanel();

            // Set/Check status every second
            m_TimedEventManager = new TimedEventManager(1000, m_Log);
            m_TimedEventManager.Begin();

            m_Log.Log(Logging.LOGTYPE.ERROR, "HandleState()");
            // Get states and set up processing
            HandleState();


            m_Log.Log(Logging.LOGTYPE.ERROR, "Setup timers");
            LoadMainTimer();

            LoadUpdateTimer();

            m_Log.Log(Logging.LOGTYPE.ERROR, "UpdateUI()");
            // Do initial GUI setup
            UpdateUI();

            m_Log.Log(Logging.LOGTYPE.ERROR, "Leaving Init Program()");
        }

        #region Setup Functions

        private void LoadPanels()
        {
            // Just add to list and SetUpPanels does the rest
            m_StatusPanel = new StatusPanel(this.m_CurrentVersion);
            m_Panels.Add(m_StatusPanel);

            m_AboutPanel = new HTMLPanel(g_GITURL + "about.html");
            m_Panels.Add(m_AboutPanel);
            m_AboutPanel.NavCurrent();

            m_ReleasePanel = new HTMLPanel(g_GITURL + "releasenotes.html");
            m_Panels.Add(m_ReleasePanel);
            m_ReleasePanel.NavCurrent();

            m_FeedbackPanel = new HTMLPanel(g_HOMEURL + "feedback.php");
            m_Panels.Add(m_FeedbackPanel);
            m_FeedbackPanel.NavCurrent();

            SetUpPanels(true, 15, 10);
        }

        // Puts all panels in the right state, at the right position
        // and adds them
        private void SetUpPanels(bool bVisible, int nTop, int nLeft)
        {
            foreach (UserControl cCur in m_Panels)
            {
                Controls.Add(cCur);
                cCur.Visible = bVisible;
                cCur.Top = nTop;
                cCur.Left = nLeft;
            }
        }

        // Loads the resources into Icon objects
        private void LoadIcons()
        {
            m_OkIcon = new Icon(GetType(), "Resources.led_green.ico");
            m_ErrorIcon = new Icon(GetType(), "Resources.led_red.ico");
            m_WarningIcon = new Icon(GetType(), "Resources.led_yellow.ico");
        }

        // Set all the controls for the status panel
        private void SetStatusPanel()
        {
            m_StatusPanel.UseOpenDNS = m_bUseOpenDNS;
            m_StatusPanel.UseDNSCrypt = m_bUseDNSCrypt;
            m_StatusPanel.UseDNSCryptTCP = m_bUseDNSCryptTCP;
            m_StatusPanel.UseInsecure = m_bUseInsecure;
        }

        // Launch main timer
        private void LoadMainTimer()
        {
            // Set up the timer for further control
            m_MainTimer = new System.Windows.Forms.Timer();
            m_MainTimer.Interval = 1000;
            m_MainTimer.Tick += new EventHandler(m_MainTimer_Tick);
            m_MainTimer.Enabled = true;
            m_MainTimer.Start();
        }

        #endregion

        // Main callback for timer event
        void m_MainTimer_Tick(object sender, EventArgs e)
        {
            // Tell service what to do based on UI
            HandleState();

            // Show user what is currently happening
            UpdateUI();
        }

        private void HandleState()
        {
            // Get changes in user input
            m_bUseOpenDNS = m_StatusPanel.UseOpenDNS;
            m_bUseDNSCrypt = m_StatusPanel.UseDNSCrypt;
            m_bUseDNSCryptTCP = m_StatusPanel.UseDNSCryptTCP;
            m_bUseInsecure = m_StatusPanel.UseInsecure;

            // Tell service what users wants
            m_TimedEventManager.SetDesiredState(m_bUseDNSCrypt, m_bUseDNSCryptTCP, m_bUseOpenDNS, m_bUseInsecure);
        }

        void UpdateUI()
        {
            // Overall state display
            ShowSystemState();

            // Make sure the correct panel is up
            ShowActivePanel();
        }

        #region Pure UI

        // Sets the system tray icon and the sheild
        private void ShowSystemState()
        {
            string sStatusMsg = "";

            if (m_TimedEventManager.GetIsStopping())
                return;

            TimedEventManager.RUN_STATE State = m_TimedEventManager.GetCurrentState();
            switch (State)
            {
                case TimedEventManager.RUN_STATE.DNSCRYPT_FULL:
                    sStatusMsg = "Status: Protected";
                    m_StatusPanel.UpdatePanelUI((int)OVERALL_STATUS.OK, sStatusMsg, (string)"208.67.220.220 Using DNSCrypt");
                    SysTrayIcon.Icon = m_OkIcon;
                    SysTrayIcon.Text = sStatusMsg;
                    break;

                case TimedEventManager.RUN_STATE.DNSCRYPT_TCP:
                    sStatusMsg = "Status: Protected";
                    m_StatusPanel.UpdatePanelUI((int)OVERALL_STATUS.OK, sStatusMsg, (string)"208.67.220.220 Using DNSCrypt/HTTPS");
                    SysTrayIcon.Icon = m_OkIcon;
                    SysTrayIcon.Text = sStatusMsg;
                    break;

                case TimedEventManager.RUN_STATE.OPENDNS_ONLY:
                    sStatusMsg = "Status: Not Encrypted";
                    m_StatusPanel.UpdatePanelUI((int)OVERALL_STATUS.WARNING, sStatusMsg, (string)"208.67.220.220");
                    SysTrayIcon.Icon = m_WarningIcon;
                    SysTrayIcon.Text = sStatusMsg;
                    break;

                case TimedEventManager.RUN_STATE.SERVICE_RUNNING:
                    sStatusMsg = "Status: Unprotected";
                    m_StatusPanel.UpdatePanelUI((int)OVERALL_STATUS.WARNING, sStatusMsg, "Changing DNS Servers...");
                    SysTrayIcon.Icon = m_WarningIcon;
                    SysTrayIcon.Text = sStatusMsg;
                    break;

                case TimedEventManager.RUN_STATE.SERVICE_RESTART:
                    sStatusMsg = "Status: Unprotected";
                    m_StatusPanel.UpdatePanelUI((int)OVERALL_STATUS.WARNING, sStatusMsg, "Changing DNS Servers...");
                    SysTrayIcon.Icon = m_WarningIcon;
                    SysTrayIcon.Text = sStatusMsg;
                    break;

                case TimedEventManager.RUN_STATE.FAIL_OPEN:
                    sStatusMsg = "Status: Unprotected";
                    m_StatusPanel.UpdatePanelUI((int)OVERALL_STATUS.BAD, sStatusMsg, "Default");
                    SysTrayIcon.Icon = m_ErrorIcon;
                    SysTrayIcon.Text = sStatusMsg;

                    /*
                    if (!m_TimedEventManager.GetHaveNetwork())
                    {
                        m_StatusPanel.UseOpenDNS = false;
                    }
                     * */

                    break;

                case TimedEventManager.RUN_STATE.FAIL_CLOSED:
                    sStatusMsg = "Status: Unprotected";
                    m_StatusPanel.UpdatePanelUI((int)OVERALL_STATUS.BAD, sStatusMsg, "Default");
                    SysTrayIcon.Icon = m_ErrorIcon;
                    SysTrayIcon.Text = sStatusMsg;
                    break;
                default:
                    sStatusMsg = "Status: No Network";
                    m_StatusPanel.UpdatePanelUI((int)OVERALL_STATUS.BAD, sStatusMsg, "None Available");
                    SysTrayIcon.Icon = m_ErrorIcon;
                    SysTrayIcon.Text = sStatusMsg;
                    break;
            }

            // We now write directly from the StatusPanel
            //WriteConfiguredStart();
        }

        // Decides which panel the user is currently looking at and ensures
        // it is showing
        private void ShowActivePanel()
        {
            switch (m_CurVisiblePanel)
            {
                case PANELS.GENERAL:
                    m_AboutPanel.Visible = false;
                    m_StatusPanel.Visible = true;
                    m_ReleasePanel.Visible = false;
                    m_FeedbackPanel.Visible = false;
                    break;

                case PANELS.ABOUT:
                    m_StatusPanel.Visible = false;
                    m_AboutPanel.Visible = true;
                    m_ReleasePanel.Visible = false;
                    m_FeedbackPanel.Visible = false;
                    break;

                case PANELS.RELEASE_NOTES:
                    m_StatusPanel.Visible = false;
                    m_AboutPanel.Visible = false;
                    m_ReleasePanel.Visible = true;
                    m_FeedbackPanel.Visible = false;
                    break;

                case PANELS.PREVIEW_FEEDBACK:
                    m_StatusPanel.Visible = false;
                    m_AboutPanel.Visible = false;
                    m_ReleasePanel.Visible = false;
                    m_FeedbackPanel.Visible = true;
                    break;
            }
        }

        #endregion

        #region Service Functions

        public void StopWindowsService()
        {
            //m_SI.ControlService(ServiceInterface.SERVICE_CONTROL.STOP);
        }

        public void StartWindowsService()
        {
            //m_SI.ControlService(ServiceInterface.SERVICE_CONTROL.START);
        }

        #endregion

        #region System Tray Icon Functions

        private void UpdaterSysIcon_MouseClick(object sender, MouseEventArgs e)
        {
            // Pop the context menu
            if (e.Button == MouseButtons.Right)
                this.SysTrayMenu.Show();
        }

        private void UpdaterSysIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // For cases when the UI is occluded by some other window
            // and the user double clicks the icon
            if (this.Visible && !this.Focused)
            {
                this.Activate();
                return;
            }

            // NOTE: hide everyone so redraw doesn't look clunky.
            // If we don't hide before we show the window, we see redraw
            m_AboutPanel.Visible = false;
            m_StatusPanel.Visible = false;
            m_ReleasePanel.Visible = false;
            m_FeedbackPanel.Visible = false;

            // Show or hide
            if (this.Visible)
                SetWindowVisible(false);
            else
                SetWindowVisible(true);

            // Now force reset of current panel
            UpdateUI();
        }

        #endregion

        #region System Tray Menu Functions

        private void SysTrayMenu_Opening(object sender, CancelEventArgs e)
        {
            // Handle things you need to do first here
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Close up shop; this will stop the service as well
            m_bExit = true;
            this.Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetWindowVisible(true);
        }

        #endregion

        #region Utility

        private void SetWindowVisible(bool bShow)
        {
            if (bShow)
            {
                this.Visible = true;
                this.WindowState = FormWindowState.Normal;

                this.BringToFront();
            }
            else
            {
                this.Visible = false;
                this.WindowState = FormWindowState.Minimized;
            }
        }

        private void StopTimer()
        {
            if (m_MainTimer != null)
            {
                if (m_MainTimer.Enabled)
                    m_MainTimer.Stop();
            }
        }

        private void StartTimer()
        {
            if (m_MainTimer != null)
            {
                if (!m_MainTimer.Enabled)
                    m_MainTimer.Start();
            }
        }

        private void ReadConfiguredStart()
        {
            try
            {
                // Get the configured start setting
                m_bUseOpenDNS = Properties.Settings.Default.UseOpenDNS;
                m_bUseDNSCrypt = Properties.Settings.Default.UseDNSCrypt;
                m_bUseDNSCryptTCP = Properties.Settings.Default.UseDNSCryptTCP;
                m_bUseInsecure = Properties.Settings.Default.UseInsecure;
            }
            catch (System.Configuration.ConfigurationErrorsException ex)
            {
                // For some reason, there are problems reading the user.config file
                m_Log.Log(Logging.LOGTYPE.ERROR, "Error in ReadConfiguredStart: " + ex.Message);

                string fileName = "";
                if (!string.IsNullOrEmpty(ex.Filename))
                    fileName = ex.Filename;
                else
                {
                    System.Configuration.ConfigurationErrorsException innerException = ex.InnerException as System.Configuration.ConfigurationErrorsException;
                    if (innerException != null && !string.IsNullOrEmpty(innerException.Filename))
                        fileName = innerException.Filename;
                }
                if (System.IO.File.Exists(fileName))
                {
                    // Delete the (empty) user.config file
                    m_Log.Log(Logging.LOGTYPE.DEBUG, "Deleting file: " + fileName);
                    System.IO.File.Delete(fileName);

                    // At this point, the program will crash.  But after a restart, it will re-create the file!
                }
            }
        }

        private void WriteConfiguredStart()
        {
            // Use settings to write to disk, this is per user

            Properties.Settings.Default.UseOpenDNS = m_bUseOpenDNS;
            Properties.Settings.Default.UseDNSCrypt = m_bUseDNSCrypt;
            Properties.Settings.Default.UseDNSCryptTCP = m_bUseDNSCryptTCP;
            Properties.Settings.Default.UseInsecure = m_bUseInsecure;

            // Must call save or it doesn't work
            Properties.Settings.Default.Save();
        }

        #endregion

        #region Update Functions

        void LoadUpdateTimer()
        {
            // Set our version label immediately
            versionLinkLabel.Text = "Version " + m_CurrentVersion;

            // Check for updates immediately on start
            CheckForProgramUpgrades(0);

            // Set up the Upgrade Timer, which is a much slower tick
            // Perhaps a Forms timer is not the best way to count off a day, but it's fine for now.
            m_UpgradeTimer = new System.Windows.Forms.Timer();
            m_UpgradeTimer.Interval = 1000 * 60 * 60;
            //m_UpgradeTimer.Interval = 1000 * 5;
            m_UpgradeTimer.Tick += new EventHandler(m_UpgradeTimer_Tick);
            m_UpgradeTimer.Start();
        }

        // Main callback for the update timer tick
        void m_UpgradeTimer_Tick(object sender, EventArgs e)
        {
            CheckForProgramUpgrades(0);
        }

        /// <summary>
        /// Checks for upgrades to the program
        /// </summary>
        void CheckForProgramUpgrades(int randomSlop)
        {
            backgroundWorkerUpdate.RunWorkerAsync();
        }

        private void backgroundWorkerUpdate_DoWork(object sender, DoWorkEventArgs e)
        {
            m_bUpgradeServerFailed = !m_Updater.CheckForUpdates();
        }

        private void backgroundWorkerUpdate_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!m_bUpgradeServerFailed)
            {
                // We've successfully checked for updates, so update our variables
                m_bUpgradeAvailable = m_Updater.UpgradeAvailable;
                if (!m_Updater.UpgradeAvailable)
                {
                    versionLinkLabel.Text = "Version " + m_CurrentVersion + " (Latest)";
                    versionLinkLabel.Enabled = false;
                }

                if (!string.IsNullOrEmpty(m_Updater.LatestVersion) && !string.IsNullOrEmpty(m_Updater.DownloadUrl))
                {
                    // Only make link active when there is actually an updated version and a download link
                    versionLinkLabel.Text = versionLinkLabel.Text = "Version " + m_CurrentVersion + string.Format(" (Updated version {0} available)", m_Updater.LatestVersion);
                    versionLinkLabel.Enabled = true;
                }

                Debug.WriteLine(string.Format("Upgrade available: {0} {1}", m_bUpgradeAvailable.ToString(), m_Updater.LatestVersion));
            }
            else
            {
                // We've failed checking for updates... I don't quite like how this goes down...
                Debug.WriteLine("Problem checking for updates!");
                m_bUpgradeAvailable = false;
                versionLinkLabel.Text = "Version " + m_CurrentVersion;
                versionLinkLabel.Enabled = false;
            }
        }

        private void backgroundWorkerDownload_DoWork(object sender, DoWorkEventArgs e)
        {
            m_bDownloadSuccess = m_Updater.DownloadUpdate();
        }

        private void backgroundWorkerDownload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            versionLinkLabel.Text = "Download complete, installing...";

            if (m_bDownloadSuccess)
            {
                // Stop the service, so that it can be updated
                m_TimedEventManager.CleanUp();

                // Start the installer
                Process.Start(m_Updater.DownloadPath);

                // Stop THIS application, so that it can be updated
                Application.Exit();
            }
        }

        private void versionLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            versionLinkLabel.Text = "Downloading " + System.IO.Path.GetFileName(m_Updater.DownloadUrl) + "...";
            versionLinkLabel.Enabled = false;
            backgroundWorkerDownload.RunWorkerAsync();
        }

        #endregion

        #region Button Functions

        private void GeneralButton_Click(object sender, EventArgs e)
        {
            m_CurVisiblePanel = PANELS.GENERAL;
            ShowActivePanel();
        }

        private void AboutButton_Click(object sender, EventArgs e)
        {
            m_CurVisiblePanel = PANELS.ABOUT;
            ShowActivePanel();
        }

        private void ReleaseButton_Click(object sender, EventArgs e)
        {
            m_CurVisiblePanel = PANELS.RELEASE_NOTES;
            ShowActivePanel();
        }

        private void PreviewButton_Click(object sender, EventArgs e)
        {
            m_CurVisiblePanel = PANELS.PREVIEW_FEEDBACK;
            ShowActivePanel();
        }

        #endregion

    }
}
