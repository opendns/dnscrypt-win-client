namespace OpenDNSInterface
{
    partial class OpenDNSInterface
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OpenDNSInterface));
            this.SysTrayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SysTrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.backgroundWorkerDownload = new System.ComponentModel.BackgroundWorker();
            this.versionLinkLabel = new System.Windows.Forms.LinkLabel();
            this.backgroundWorkerUpdate = new System.ComponentModel.BackgroundWorker();
            this.MainButtonStrip = new System.Windows.Forms.ToolStrip();
            this.GeneralButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.AboutButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.ReleaseButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.PreviewButton = new System.Windows.Forms.ToolStripButton();
            this.SysTrayMenu.SuspendLayout();
            this.MainButtonStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // SysTrayMenu
            // 
            this.SysTrayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.SysTrayMenu.Name = "SysTrayMenu";
            this.SysTrayMenu.Size = new System.Drawing.Size(185, 48);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.openToolStripMenuItem.Text = "Open Control Center";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // SysTrayIcon
            // 
            this.SysTrayIcon.ContextMenuStrip = this.SysTrayMenu;
            this.SysTrayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("SysTrayIcon.Icon")));
            this.SysTrayIcon.Text = "OpenDNS";
            this.SysTrayIcon.Visible = true;
            this.SysTrayIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.UpdaterSysIcon_MouseDoubleClick);
            // 
            // backgroundWorkerDownload
            // 
            this.backgroundWorkerDownload.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerDownload_DoWork);
            this.backgroundWorkerDownload.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerDownload_RunWorkerCompleted);
            // 
            // versionLinkLabel
            // 
            this.versionLinkLabel.AutoSize = true;
            this.versionLinkLabel.DisabledLinkColor = System.Drawing.SystemColors.ControlText;
            this.versionLinkLabel.Enabled = false;
            this.versionLinkLabel.Location = new System.Drawing.Point(277, 365);
            this.versionLinkLabel.Name = "versionLinkLabel";
            this.versionLinkLabel.Size = new System.Drawing.Size(60, 13);
            this.versionLinkLabel.TabIndex = 7;
            this.versionLinkLabel.TabStop = true;
            this.versionLinkLabel.Text = "Version 0.7";
            this.versionLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.versionLinkLabel_LinkClicked);
            // 
            // backgroundWorkerUpdate
            // 
            this.backgroundWorkerUpdate.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerUpdate_DoWork);
            this.backgroundWorkerUpdate.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerUpdate_RunWorkerCompleted);
            // 
            // MainButtonStrip
            // 
            this.MainButtonStrip.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.MainButtonStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.MainButtonStrip.GripMargin = new System.Windows.Forms.Padding(0);
            this.MainButtonStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.MainButtonStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.GeneralButton,
            this.toolStripSeparator1,
            this.AboutButton,
            this.toolStripSeparator2,
            this.ReleaseButton,
            this.toolStripSeparator3,
            this.PreviewButton});
            this.MainButtonStrip.Location = new System.Drawing.Point(143, 12);
            this.MainButtonStrip.Name = "MainButtonStrip";
            this.MainButtonStrip.Size = new System.Drawing.Size(351, 26);
            this.MainButtonStrip.TabIndex = 10;
            this.MainButtonStrip.Text = "toolStrip1";
            // 
            // GeneralButton
            // 
            this.GeneralButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.GeneralButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.GeneralButton.Image = ((System.Drawing.Image)(resources.GetObject("GeneralButton.Image")));
            this.GeneralButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.GeneralButton.Name = "GeneralButton";
            this.GeneralButton.Size = new System.Drawing.Size(60, 23);
            this.GeneralButton.Text = "General";
            this.GeneralButton.Click += new System.EventHandler(this.GeneralButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 26);
            // 
            // AboutButton
            // 
            this.AboutButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.AboutButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.AboutButton.Image = ((System.Drawing.Image)(resources.GetObject("AboutButton.Image")));
            this.AboutButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.AboutButton.Name = "AboutButton";
            this.AboutButton.Size = new System.Drawing.Size(51, 23);
            this.AboutButton.Text = "About";
            this.AboutButton.Click += new System.EventHandler(this.AboutButton_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 26);
            // 
            // ReleaseButton
            // 
            this.ReleaseButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.ReleaseButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.ReleaseButton.Image = ((System.Drawing.Image)(resources.GetObject("ReleaseButton.Image")));
            this.ReleaseButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ReleaseButton.Name = "ReleaseButton";
            this.ReleaseButton.Size = new System.Drawing.Size(98, 23);
            this.ReleaseButton.Text = "Release Notes";
            this.ReleaseButton.Click += new System.EventHandler(this.ReleaseButton_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 26);
            // 
            // PreviewButton
            // 
            this.PreviewButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.PreviewButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.PreviewButton.Image = ((System.Drawing.Image)(resources.GetObject("PreviewButton.Image")));
            this.PreviewButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PreviewButton.Name = "PreviewButton";
            this.PreviewButton.Size = new System.Drawing.Size(121, 23);
            this.PreviewButton.Text = "Preview Feedback";
            this.PreviewButton.Click += new System.EventHandler(this.PreviewButton_Click);
            // 
            // OpenDNSInterface
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.ClientSize = new System.Drawing.Size(636, 387);
            this.ContextMenuStrip = this.SysTrayMenu;
            this.Controls.Add(this.MainButtonStrip);
            this.Controls.Add(this.versionLinkLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "OpenDNSInterface";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "DNS Crypt";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.Load += new System.EventHandler(this.OpenDNSInterface_Load);
            this.SysTrayMenu.ResumeLayout(false);
            this.MainButtonStrip.ResumeLayout(false);
            this.MainButtonStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip SysTrayMenu;
        private System.Windows.Forms.NotifyIcon SysTrayIcon;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.ComponentModel.BackgroundWorker backgroundWorkerDownload;
        private System.Windows.Forms.LinkLabel versionLinkLabel;
        private System.ComponentModel.BackgroundWorker backgroundWorkerUpdate;
        private System.Windows.Forms.ToolStrip MainButtonStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton GeneralButton;
        private System.Windows.Forms.ToolStripButton AboutButton;
        private System.Windows.Forms.ToolStripButton ReleaseButton;
        private System.Windows.Forms.ToolStripButton PreviewButton;
    }
}

