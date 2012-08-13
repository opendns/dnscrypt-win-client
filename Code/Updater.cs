using System;
using System.Net;
using System.IO;
using System.Collections;
using System.Diagnostics;


public class Updater
{
    const string URL_BASE = @"https://raw.github.com/opendns/dnscrypt-win-client/master/";
    const string PROGRAM_KEY = @"DNSCryptUpgrade/";

    #region Private members

    string m_CurrentVersion = "";
    string m_LatestVersion = string.Empty;
    bool m_bUpgradeAvailable = true;
    bool m_bForceUpgrade = false;
    string m_DownloadUrl = string.Empty;
    bool m_bUpdateInProgress = false;
    bool m_bDownloadInProgress = false;
    string m_DownloadDir = string.Empty;
    string m_DownloadPath = string.Empty;

    #endregion

    #region Public Properties

    public string LatestVersion { get { return m_LatestVersion; } }
    public bool UpgradeAvailable { get { return m_bUpgradeAvailable; } }
    public bool ForceUpgrade { get { return m_bForceUpgrade; } }
    public string DownloadUrl { get { return m_DownloadUrl; } }
    public string DownloadPath { get { return m_DownloadPath; } }

    #endregion


    /// <summary>
    /// Constructor
    /// </summary>
    public Updater(string sCurVersion) {
        m_DownloadDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "OpenDNS");
        m_DownloadDir = Path.Combine(m_DownloadDir, "Client Updates");
        Directory.CreateDirectory(m_DownloadDir);
        m_CurrentVersion = sCurVersion;
    }

    /// <summary>
    /// Queries the OpenDNS webpage for update status and sets property members accordingly. 
    /// This method is synchronous and could block for a time.  It should be called from a non-UI thread.
    /// </summary>
    /// <returns>Returns true if the check was successful, false if it was unsuccessful for any reason (no response, malformed response, missing fields)</returns>
    public bool CheckForUpdates()
    {
        if (m_bUpdateInProgress || m_bDownloadInProgress)
        {
            Debug.WriteLine("Aborting CheckForUpdates at " + DateTime.Now.ToString());
            return false;
        }

        Debug.WriteLine("Running CheckForUpdates at " + DateTime.Now.ToString());

        string jsonStr = GetUpgradeStringFromServer();

        if (string.IsNullOrEmpty(jsonStr))
        {
            Debug.WriteLine("Upgrade string is empty.");
            return false;
        }

        bool success = false;
        Object jsonResp = JSON.JsonDecode(jsonStr, ref success);
        if (success && (jsonResp is Hashtable))
        {
            Hashtable hResp = jsonResp as Hashtable;
            if (hResp.ContainsKey("version"))
            {
                m_bUpgradeAvailable = false;
                string sAvailableVersion = (string)hResp["version"];
                m_bUpgradeAvailable = IsNewVersionAvailable(sAvailableVersion, m_CurrentVersion); 
                if (!m_bUpgradeAvailable)
                {
                    // If there is no upgrade, make sure everything else is emptied out
                    m_bForceUpgrade = false;
                    m_LatestVersion = string.Empty;
                    m_DownloadUrl = string.Empty;
                }
            }
            else
            {
                // We got a valid JSON response, but it didn't have an "upgrade" key... just punt for now
                return false;
            }

            if (m_bUpgradeAvailable)
            {
                if (hResp.ContainsKey("download"))
                {
                    m_DownloadUrl = (string)hResp["download"];
                }
                else
                {
                    m_DownloadUrl = string.Empty;
                    return false;
                }

                if (hResp.ContainsKey("version"))
                {
                    m_LatestVersion = (string)hResp["version"];
                }
                else
                {
                    // Can't upgrade with no version
                    m_LatestVersion = string.Empty;
                    return false;
                }
            }
        }
        else
        {
            return false;
        }


        return true;
    }

    private bool IsNewVersionAvailable(string sAvailable, string sCurVersion)
    {
        Version Avail = new Version(sAvailable);
        Version Cur = new Version(sCurVersion);

        if (Avail > Cur)
        {
            return true;
        }

        return false;
    }

    public bool DownloadUpdate()
    {
        if (string.IsNullOrEmpty(m_DownloadUrl) || m_bDownloadInProgress)
        {
            return false;
        }
        
        m_bDownloadInProgress = true;
        WebRequest wrReq = WebRequest.Create(m_DownloadUrl);
        wrReq.Timeout = 60000; // timeout in 1 minute

        try
        {
            WebResponse wrResp = wrReq.GetResponse();
            WebClient wcClient = new WebClient();

            // Not going to bother with progress bars or going async yet...
            m_DownloadPath = Path.Combine(m_DownloadDir, Path.GetFileName(m_DownloadUrl));

            if (!File.Exists(m_DownloadPath))
            {
                wcClient.DownloadFile(m_DownloadUrl, m_DownloadPath);
            }
            
            return true;
        }
        catch (WebException ex)
        {
            Debug.WriteLine(ex.ToString());
            return false;
        }
    }

    /// <summary>
    /// Gets the JSON response from the web server.
    /// </summary>
    /// <returns>Returns a string representing the JSON object on success, or an empty string on failure.</returns>
    private string GetUpgradeStringFromServer()
    {
        // TODO: support for stats variables (t, i, u, c, l, osver, tec, etc)
        //string updateUri = URL_BASE + PROGRAM_KEY + "?version=" + VersionInfo.UpdateVersion;
        string updateUri = URL_BASE + PROGRAM_KEY + "ver_manifest.txt";

        WebRequest wrReq = WebRequest.Create(updateUri);
        wrReq.Timeout = 60000; // timeout in 1 minute

        try
        {
            // Synchronous access should be fine, as this should always be run on a separate thread
            WebResponse wrResp = wrReq.GetResponse();
            StreamReader reader = new StreamReader(wrResp.GetResponseStream());
            string respFromServer = reader.ReadToEnd();
            return respFromServer;
        }
        catch (WebException ex)
        {
            // If we don't make our timeout, print out the debug and return an empty string to signify an error
            Debug.WriteLine(ex.ToString());
            return string.Empty;
        }
    }

    /// <summary>
    /// Deletes everything from the Download Directory
    /// </summary>
    private void CleanDownloadDir()
    {
        foreach (string file in Directory.GetFiles(m_DownloadDir))
        {
            File.Delete(file);
        }
    }
}
