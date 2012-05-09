using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.IO.Log;

public class Logging
{
    public enum LOGTYPE { ERROR = 0, INFO, DEBUG };

    bool m_bFileBased = false;
    LogRecordSequence m_LogRS = null;
    FileStream m_LogFS = null;

    static string m_sFileName = "OpenDNS_DNSCrypt_Client";
    static string m_sFileExt = ".log";

    static LOGTYPE m_Level = LOGTYPE.ERROR;

    // Set up newline
    byte[] m_bNewnline = ASCIIEncoding.Default.GetBytes(Environment.NewLine);

    public Logging(string sFileName, string sLocation, bool bFileBased)
    {
        m_bFileBased = bFileBased;
        m_sFileName = sFileName;

        if (sLocation.Length > 0)
            if (sLocation[sLocation.Length - 1] != '\\')
                sLocation += "\\";

        if (m_bFileBased)
        {
            m_LogFS = OpenFile(sLocation, m_sFileName, m_sFileExt);
        }
        else
        {
            string sPath = sLocation + m_sFileName + m_sFileExt;
            m_LogRS = new LogRecordSequence(sPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
            m_LogRS.LogStore.Extents.Add("app_extent0", 32 * 1024);
            m_LogRS.LogStore.Extents.Add("app_extent1");

            // Set up auto-grow and such
            m_LogRS.LogStore.Policy.AutoGrow = true;
            m_LogRS.LogStore.Policy.MaximumExtentCount = 6;
            m_LogRS.LogStore.Policy.GrowthRate = new PolicyUnit(5, PolicyUnitType.Extents);
            m_LogRS.LogStore.Policy.Commit();
            m_LogRS.LogStore.Policy.Refresh();
        }

        // Set up
        Log(Logging.LOGTYPE.ERROR, "Start logging...");
    }

    ~Logging()
    {
        if (m_bFileBased)
        {
            m_LogFS.Close();
        }
        else
        {
            m_LogRS.Dispose();
        }
    }

    public void SetLogging(LOGTYPE Level)
    {
        m_Level = Level;
    }

    public void Log(LOGTYPE Type, string sData)
    {
        if (Type > m_Level)
            return;

        // Do some stuff that always applies
        sData = GetDateTime() + sData;

        System.Diagnostics.Debug.WriteLine(m_sFileName + ": " + sData);

        if (m_bFileBased)
            LogToFile(Type, sData);
        else
            LogToStore(Type, sData);
    }

    // Overload that defaults to Debug Level
    public void Log(string sPrepend, string sData)
    {
        if (LOGTYPE.DEBUG > m_Level)
            return;

        // Do some stuff that always applies
        sData = GetDateTime() + sPrepend + "-" + sData;

        System.Diagnostics.Debug.WriteLine(m_sFileName + ": " + sData);

        if (m_bFileBased)
            LogToFile(LOGTYPE.DEBUG, sData);
        else
            LogToStore(LOGTYPE.DEBUG, sData);
    }

    #region Log Functions

    // Creates a file with the input params, if not found, creates the file
    private FileStream OpenFile(string sLocation, string sFileName, string sFileExt)
    {
        string sPath = sLocation + sFileName + MakeFileDate() + sFileExt;
        return new FileStream(sPath, FileMode.Append);
    }

    // File version
    private void LogToFile(LOGTYPE Type, string sData)
    {
        byte[] bBuf = ASCIIEncoding.Default.GetBytes(sData);

        lock (m_LogFS)
        {
            m_LogFS.Write(bBuf, 0, bBuf.Length);
            m_LogFS.Write(m_bNewnline, 0, m_bNewnline.Length);
            m_LogFS.Flush();
        }
    }

    // Event Log version
    private void LogToStore(LOGTYPE Type, string sData)
    {
        lock (m_LogRS)
        {
            SequenceNumber Last = SequenceNumber.Invalid;
            Last = m_LogRS.Append(CreateData(sData), SequenceNumber.Invalid, SequenceNumber.Invalid, RecordAppendOptions.ForceFlush);
        }
    }

    #endregion

    #region Utility

    // Convert into the silly template format they require for the log class
    private static IList<ArraySegment<byte>> CreateData(string sData)
    {
        Encoding Enc = Encoding.Default;

        byte[] Data = Enc.GetBytes(sData);

        ArraySegment<byte>[] Segments = new ArraySegment<byte>[1];

        Segments[0] = new ArraySegment<byte>(Data);

        return Array.AsReadOnly<ArraySegment<byte>>(Segments);
    }

    private string GetDateTime()
    {
        string sOut = DateTime.UtcNow.ToShortDateString() + " " + DateTime.UtcNow.ToLongTimeString();
        sOut += ": ";
        return sOut;
    }

    private string MakeFileDate()
    {
        return "_" + DateTime.UtcNow.ToShortDateString().Replace('/', '_');
    }

    #endregion
}
