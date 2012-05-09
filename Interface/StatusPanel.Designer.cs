namespace OpenDNSInterface
{
    partial class StatusPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StatusPanel));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.OpenDNSLink = new System.Windows.Forms.LinkLabel();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.CurrentIPLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.DescriptionLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.EnableInsecure = new System.Windows.Forms.CheckBox();
            this.EnableDNSCryptTCP = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.StatusImage = new System.Windows.Forms.PictureBox();
            this.EnableDNSCrypt = new System.Windows.Forms.CheckBox();
            this.EnableOpenDNS = new System.Windows.Forms.CheckBox();
            this.OpenDNSLogo = new System.Windows.Forms.PictureBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.StatusImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.OpenDNSLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.OpenDNSLink);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.CurrentIPLabel);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.DescriptionLabel);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.EnableInsecure);
            this.groupBox1.Controls.Add(this.EnableDNSCryptTCP);
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.StatusLabel);
            this.groupBox1.Controls.Add(this.StatusImage);
            this.groupBox1.Controls.Add(this.EnableDNSCrypt);
            this.groupBox1.Controls.Add(this.EnableOpenDNS);
            this.groupBox1.Controls.Add(this.OpenDNSLogo);
            this.groupBox1.Location = new System.Drawing.Point(10, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(587, 338);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            // 
            // OpenDNSLink
            // 
            this.OpenDNSLink.AutoSize = true;
            this.OpenDNSLink.BackColor = System.Drawing.Color.White;
            this.OpenDNSLink.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OpenDNSLink.LinkColor = System.Drawing.Color.Black;
            this.OpenDNSLink.Location = new System.Drawing.Point(351, 122);
            this.OpenDNSLink.Name = "OpenDNSLink";
            this.OpenDNSLink.Size = new System.Drawing.Size(160, 17);
            this.OpenDNSLink.TabIndex = 17;
            this.OpenDNSLink.TabStop = true;
            this.OpenDNSLink.Text = "http://www.opendns.com";
            this.OpenDNSLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OpenDNSLink_LinkClicked);
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(321, 269);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(222, 36);
            this.label8.TabIndex = 16;
            this.label8.Text = "If you prefer reliability over security, enable fallback to insecure DNS";
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(321, 226);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(241, 50);
            this.label7.TabIndex = 15;
            this.label7.Text = "If you have a firewall or other middleware mangling your packets, try enabling DN" +
    "SCrypt with TCP over port 443.";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(321, 209);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(207, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Please help by providing feedback!";
            // 
            // CurrentIPLabel
            // 
            this.CurrentIPLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CurrentIPLabel.Location = new System.Drawing.Point(18, 280);
            this.CurrentIPLabel.Name = "CurrentIPLabel";
            this.CurrentIPLabel.Size = new System.Drawing.Size(169, 42);
            this.CurrentIPLabel.TabIndex = 13;
            this.CurrentIPLabel.Text = "208.67.220.220 using DNSCrypt/HTTPS";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(17, 256);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(170, 20);
            this.label4.TabIndex = 12;
            this.label4.Text = "Current DNS Resolver:";
            // 
            // DescriptionLabel
            // 
            this.DescriptionLabel.Location = new System.Drawing.Point(321, 147);
            this.DescriptionLabel.Name = "DescriptionLabel";
            this.DescriptionLabel.Size = new System.Drawing.Size(241, 57);
            this.DescriptionLabel.TabIndex = 11;
            this.DescriptionLabel.Text = "This software (v: <VERSION>) encrypts DNS packets between your computer and OpenD" +
    "NS. This prevents man-in-the-middle attacks and snooping of DNS traffic by ISPs " +
    "or others.";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(16, 209);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(250, 60);
            this.label2.TabIndex = 10;
            this.label2.Text = "Note: Only enable TCP/443 if you use lots of commercial Wi-Fi hotspots at places " +
    "like Starbucks or GoGo-Inflight Wifi.";
            // 
            // EnableInsecure
            // 
            this.EnableInsecure.AutoSize = true;
            this.EnableInsecure.Checked = true;
            this.EnableInsecure.CheckState = System.Windows.Forms.CheckState.Checked;
            this.EnableInsecure.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EnableInsecure.Location = new System.Drawing.Point(20, 180);
            this.EnableInsecure.Name = "EnableInsecure";
            this.EnableInsecure.Size = new System.Drawing.Size(211, 24);
            this.EnableInsecure.TabIndex = 9;
            this.EnableInsecure.Text = "Fall back to insecure DNS";
            this.EnableInsecure.UseVisualStyleBackColor = true;
            this.EnableInsecure.CheckedChanged += new System.EventHandler(this.EnableInsecure_CheckedChanged);
            // 
            // EnableDNSCryptTCP
            // 
            this.EnableDNSCryptTCP.AutoSize = true;
            this.EnableDNSCryptTCP.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EnableDNSCryptTCP.Location = new System.Drawing.Point(20, 156);
            this.EnableDNSCryptTCP.Name = "EnableDNSCryptTCP";
            this.EnableDNSCryptTCP.Size = new System.Drawing.Size(265, 24);
            this.EnableDNSCryptTCP.TabIndex = 8;
            this.EnableDNSCryptTCP.Text = "DNSCrypt over TCP / 443 (slower)";
            this.EnableDNSCryptTCP.UseVisualStyleBackColor = true;
            this.EnableDNSCryptTCP.CheckedChanged += new System.EventHandler(this.EnableDNSCryptTCP_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Location = new System.Drawing.Point(290, 17);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(4, 305);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Location = new System.Drawing.Point(18, 136);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(258, 10);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            // 
            // StatusLabel
            // 
            this.StatusLabel.AutoSize = true;
            this.StatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StatusLabel.Location = new System.Drawing.Point(55, 34);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(95, 25);
            this.StatusLabel.TabIndex = 5;
            this.StatusLabel.Text = "Protected";
            // 
            // StatusImage
            // 
            this.StatusImage.Image = global::OpenDNSInterface.Properties.Resources.shield_green;
            this.StatusImage.Location = new System.Drawing.Point(20, 27);
            this.StatusImage.Name = "StatusImage";
            this.StatusImage.Size = new System.Drawing.Size(32, 32);
            this.StatusImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.StatusImage.TabIndex = 4;
            this.StatusImage.TabStop = false;
            // 
            // EnableDNSCrypt
            // 
            this.EnableDNSCrypt.AutoSize = true;
            this.EnableDNSCrypt.Checked = true;
            this.EnableDNSCrypt.CheckState = System.Windows.Forms.CheckState.Checked;
            this.EnableDNSCrypt.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EnableDNSCrypt.Location = new System.Drawing.Point(20, 103);
            this.EnableDNSCrypt.Name = "EnableDNSCrypt";
            this.EnableDNSCrypt.Size = new System.Drawing.Size(153, 24);
            this.EnableDNSCrypt.TabIndex = 2;
            this.EnableDNSCrypt.Text = "Enable DNSCrypt";
            this.EnableDNSCrypt.UseVisualStyleBackColor = true;
            this.EnableDNSCrypt.CheckedChanged += new System.EventHandler(this.EnableDNSCrypt_CheckedChanged);
            // 
            // EnableOpenDNS
            // 
            this.EnableOpenDNS.AutoSize = true;
            this.EnableOpenDNS.Checked = true;
            this.EnableOpenDNS.CheckState = System.Windows.Forms.CheckState.Checked;
            this.EnableOpenDNS.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EnableOpenDNS.Location = new System.Drawing.Point(20, 74);
            this.EnableOpenDNS.Name = "EnableOpenDNS";
            this.EnableOpenDNS.Size = new System.Drawing.Size(155, 24);
            this.EnableOpenDNS.TabIndex = 1;
            this.EnableOpenDNS.Text = "Enable OpenDNS";
            this.EnableOpenDNS.UseVisualStyleBackColor = true;
            this.EnableOpenDNS.CheckedChanged += new System.EventHandler(this.EnableOpenDNS_CheckedChanged);
            // 
            // OpenDNSLogo
            // 
            this.OpenDNSLogo.Image = global::OpenDNSInterface.Properties.Resources.opendns_logo_300;
            this.OpenDNSLogo.InitialImage = ((System.Drawing.Image)(resources.GetObject("OpenDNSLogo.InitialImage")));
            this.OpenDNSLogo.Location = new System.Drawing.Point(316, 22);
            this.OpenDNSLogo.Name = "OpenDNSLogo";
            this.OpenDNSLogo.Size = new System.Drawing.Size(241, 92);
            this.OpenDNSLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.OpenDNSLogo.TabIndex = 0;
            this.OpenDNSLogo.TabStop = false;
            // 
            // StatusPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.groupBox1);
            this.Name = "StatusPanel";
            this.Size = new System.Drawing.Size(607, 350);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.StatusImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.OpenDNSLogo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.PictureBox OpenDNSLogo;
        private System.Windows.Forms.CheckBox EnableDNSCrypt;
        private System.Windows.Forms.CheckBox EnableOpenDNS;
        private System.Windows.Forms.PictureBox StatusImage;
        private System.Windows.Forms.Label StatusLabel;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox EnableInsecure;
        private System.Windows.Forms.CheckBox EnableDNSCryptTCP;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label DescriptionLabel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label CurrentIPLabel;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.LinkLabel OpenDNSLink;

    }
}
