using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace OpenDNSInterface
{
    public partial class StatusPanel : UserControl
    {
        #region Members

        Bitmap m_OkStatus = null;
        Bitmap m_ErrorStatus = null;
        Bitmap m_WarningStatus = null;

        #endregion

        public StatusPanel(string sCurVersion)
        {
            InitializeComponent();

            LoadImages();

            // Update the text for the version
            DescriptionLabel.Text = DescriptionLabel.Text.Replace("<VERSION>", sCurVersion);
        }

        public void UpdatePanelUI(int nServiceStatus, string sStatusLabel, string sIPLabel)
        {
            this.CurrentIPLabel.Text = sIPLabel;

            if (nServiceStatus == 0)
            {
                this.StatusImage.Image = m_OkStatus;
                this.StatusLabel.Text = sStatusLabel;
            }
            else if (nServiceStatus == 1)
            {
                this.StatusImage.Image = m_WarningStatus;
                this.StatusLabel.Text = sStatusLabel;
            }
            else
            {
                this.StatusImage.Image = m_ErrorStatus;
                this.StatusLabel.Text = sStatusLabel;
            }
        }


        #region Accessors

        public bool UseOpenDNS
        {
            get
            {
                return this.EnableOpenDNS.Checked;
            }
            set
            {
                this.EnableOpenDNS.Checked = value;
            }
        }

        public bool UseDNSCrypt
        {
            get
            {
                return this.EnableDNSCrypt.Checked;
            }
            set
            {
                this.EnableDNSCrypt.Checked = value;
            }
        }

        public bool UseDNSCryptTCP
        {
            get
            {
                return this.EnableDNSCryptTCP.Checked;
            }
            set
            {
                this.EnableDNSCryptTCP.Checked = value;
            }
        }

        public bool UseInsecure
        {
            get
            {
                return this.EnableInsecure.Checked;
            }
            set
            {
                this.EnableInsecure.Checked = value;
            }
        }

        #endregion

        private void LoadImages()
        {
            m_OkStatus = new Bitmap(GetType(), "Resources.shield_green.png");
            m_ErrorStatus = new Bitmap(GetType(), "Resources.shield_red.png");
            m_WarningStatus = new Bitmap(GetType(), "Resources.shield_yellow.png");
        }

        private void OpenDNSLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string sURL = "http://www.opendns.com";
            System.Diagnostics.Process.Start(sURL);
        }

        private void EnableDNSCrypt_CheckedChanged(object sender, EventArgs e)
        {
            // OpenDNSMirrors whatever DNSCrypt does
            if(EnableDNSCrypt.Checked)
                EnableOpenDNS.Checked = EnableDNSCrypt.Checked;

            // Can't use TCP version without using DNSCrypt overall
            if (!EnableDNSCrypt.Checked)
                EnableDNSCryptTCP.Checked = false;

            WriteConfig();
        }

        private void EnableDNSCryptTCP_CheckedChanged(object sender, EventArgs e)
        {
            if (EnableDNSCryptTCP.Checked)
            {
                // Check both using DNSCrypt and OpenDNS
                EnableDNSCrypt.Checked = true;
                EnableOpenDNS.Checked = true;
            }

            WriteConfig();
        }

        private void EnableOpenDNS_CheckedChanged(object sender, EventArgs e)
        {
            // Not using OpenDNS means not using anything
            if (!EnableOpenDNS.Checked)
            {
                EnableDNSCrypt.Checked = EnableOpenDNS.Checked;
                EnableDNSCryptTCP.Checked = EnableOpenDNS.Checked;
            }

            WriteConfig();
        }

        private void EnableInsecure_CheckedChanged(object sender, EventArgs e)
        {
            WriteConfig();
        }

        private void WriteConfig()
        {
            Properties.Settings.Default.UseOpenDNS = EnableOpenDNS.Checked;
            Properties.Settings.Default.UseDNSCrypt = EnableDNSCrypt.Checked;
            Properties.Settings.Default.UseDNSCryptTCP = EnableDNSCryptTCP.Checked;
            Properties.Settings.Default.UseInsecure = EnableInsecure.Checked;

            // Must call save or it doesn't work
            Properties.Settings.Default.Save();
        }
    }
}
