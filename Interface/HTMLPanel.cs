using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

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

        public void AddDetailsHelper()
        {
            this.MainBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(AddDetails);
        }

        private void AddDetails(object sender, EventArgs e)
        {
            WebBrowser browser = (WebBrowser) sender;
            HtmlElement form = browser.Document.Forms[0];
            HtmlElement input = null;

            Process[] processlist = null;
            String text = null;
                        
            //DNSCrypt Version
            HtmlElement div = null;
            div = form.Document.CreateElement("div");
            div.SetAttribute("style", "display:none");
            input = form.Document.CreateElement("input");
            input.SetAttribute("name", "Version");
            input.SetAttribute("value", "DNSCrypt v" + VersionInfo.UpdateVersion);
            input.SetAttribute("type", "hidden");
            div.AppendChild(input);
            form.AppendChild(div);

            //Process list with CPU
            input = form.Document.CreateElement("input");
            input.SetAttribute("name", "Processes");
            processlist = Process.GetProcesses();
            foreach (Process p in processlist)
            {
                try
                {
                    text += p.ProcessName + ": " + Math.Round(p.TotalProcessorTime.TotalSeconds, 2) + ", ";
                    text += "\n";
                }
                catch (Exception ex)
                {
                    String s = ex.Message;
                }
            }
            text = (text.Length - 3 < 0) ? text : text.Remove(text.Length - 3);
            input.SetAttribute("value", text);
            input.SetAttribute("type", "hidden");
            form.AppendChild(input);
            
        }
    }
}
