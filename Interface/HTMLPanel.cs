using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace OpenDNSInterface
{
    public partial class HTMLPanel : UserControl
    {
        string m_sURL = "";

        public HTMLPanel(string sURL)
        {
            m_sURL = sURL;
            InitializeComponent();
        }

        public void NavCurrent()
        {
            this.MainBrowser.Navigate(m_sURL);
        }
    }
}
