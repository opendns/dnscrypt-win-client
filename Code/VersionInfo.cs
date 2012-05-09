using System;
using System.Reflection;
using System.Diagnostics;

/// <summary>
/// Static class for handling Version information
/// </summary>
public static class VersionInfo
{
    private static string m_UpdateVersion;

    /// <summary>
    /// Returns the version information as an "update-friendly" string. 
    /// The format is Major.Minor.Build (ie. "1.2.3").  Revision is ignored.
    /// </summary>
    public static string UpdateVersion { get { return m_UpdateVersion; } }

    static VersionInfo()
    {
        // Grab the AssemblyFileVersion attribute and make our version string out of it
        Assembly asm = Assembly.GetExecutingAssembly();
        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
        m_UpdateVersion = string.Format("{0}.{1}.{2}", fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart);
    }
}
