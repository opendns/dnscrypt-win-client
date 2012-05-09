using System;
using System.Diagnostics;

public static class ProcessManager
{
    public static int LaunchProcess(string sProcessName, string sCommandArgs)
    {
        Process CurProcess = new Process();
        CurProcess.StartInfo.UseShellExecute = false;

        // NOTE!!! These two lines will cause the program to hang indefinitely!!!!
        //CurProcess.StartInfo.RedirectStandardOutput = true;
        //CurProcess.StartInfo.RedirectStandardError = true;

        CurProcess.EnableRaisingEvents = true;
        //CurProcess.Exited += new EventHandler(CurProcess);
        CurProcess.StartInfo.CreateNoWindow = true;
        CurProcess.StartInfo.FileName = sProcessName;
        CurProcess.StartInfo.Arguments = sCommandArgs;

        return (!CurProcess.Start()) ? 0 : 1;
    }

    public static int ReLaunchProcess(string sProcessName, string sCommandArgs)
    {
        if (KillExistingProcesses(sProcessName) == 0)
        {
            return LaunchProcess(sProcessName, sCommandArgs);
        }
        else
        {
            return 1;
        }
    }

    public static int KillExistingProcesses(string sProcessName)
    {
        int nFail = 0;

        string sCleanName = System.IO.Path.GetFileNameWithoutExtension(sProcessName);
        Process[] List = Process.GetProcessesByName(sCleanName);
        foreach (Process CurProc in List)
        {
            try
            {
                CurProc.Kill();

            }
            catch (Exception Ex)
            {
                Console.WriteLine("Failed to kill: " + CurProc.ProcessName);

                nFail += 1;
            }
        }

        Console.WriteLine("Failed to kill: " + nFail);

        return nFail;
    }

    public static int ProcessExists(string sProcessName)
    {
        string sCleanName = System.IO.Path.GetFileNameWithoutExtension(sProcessName);
        Process[] List = Process.GetProcessesByName(sCleanName);
        return List.Length;
    }

}
