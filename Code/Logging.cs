using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.IO.Log;

public class Logging
{
    public enum LOGTYPE { ERROR = 0, INFO, DEBUG };

    private static readonly log4net.ILog m_ILog = log4net.LogManager.GetLogger("TestLogger");
    private log4net.Appender.RollingFileAppender m_RFL = null;

    static string m_sFileName = "OpenDNS_DNSCrypt_Client";
    static string m_sFileExt = ".log";

    static LOGTYPE m_Level = LOGTYPE.ERROR;
      
    public Logging(string sFileName, string sLocation, bool bUseLogging)
    {
        m_sFileName = sFileName;
        if (!bUseLogging) return;

        if (sLocation.Length > 0)
            if (sLocation[sLocation.Length - 1] != '\\')
                sLocation += "\\";

        m_RFL = new log4net.Appender.RollingFileAppender();
        m_RFL.File = sLocation + m_sFileName + m_sFileExt;
        m_RFL.StaticLogFileName = true;
        m_RFL.AppendToFile = true;
        m_RFL.RollingStyle = log4net.Appender.RollingFileAppender.RollingMode.Size;
        m_RFL.MaximumFileSize = "10mb";
        m_RFL.MaxSizeRollBackups = 2;
        m_RFL.Threshold = log4net.Core.Level.All;

        //m_RFL.CountDirection = 1;
        //m_RFL.DatePattern = "HH:MM::SS"; 
        log4net.Layout.PatternLayout layout = new log4net.Layout.PatternLayout("%message%newline");
        layout.ActivateOptions();
        log4net.Filter.LevelRangeFilter filter = new log4net.Filter.LevelRangeFilter();
        filter.LevelMax = log4net.Core.Level.Emergency;
        filter.LevelMin = log4net.Core.Level.All;
        m_RFL.AddFilter(filter);
        m_RFL.Layout = layout;
        m_RFL.ActivateOptions();

        log4net.Config.BasicConfigurator.Configure(m_RFL);

        // Set up
        Log(Logging.LOGTYPE.ERROR, "Start logging...");

    }

    ~Logging()
    {
        if (m_RFL != null) m_RFL.Close();
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

        LogToFile(Type, sData);
    }

    // Overload that defaults to Debug Level
    public void Log(string sPrepend, string sData)
    {
        if (LOGTYPE.DEBUG > m_Level)
            return;

        // Do some stuff that always applies
        sData = GetDateTime() + sPrepend + "-" + sData;

        System.Diagnostics.Debug.WriteLine(m_sFileName + ": " + sData);

        LogToFile(LOGTYPE.DEBUG, sData);
    }

    #region Log Functions

    // File version
    private void LogToFile(LOGTYPE Type, string sData)
    {
        switch (Type)
        {
            case LOGTYPE.INFO:
                m_ILog.Info(sData);
                break;
            case LOGTYPE.DEBUG:
                m_ILog.Debug(sData);
                break;
            case LOGTYPE.ERROR:
                m_ILog.Error(sData);
                break;
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
