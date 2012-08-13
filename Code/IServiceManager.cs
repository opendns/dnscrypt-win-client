using System;

public interface IServiceManager
{
    bool IsServiceRunning();
    bool IsServiceRunningInTcpMode();
    bool IsProxyRunning();

    void KillServiceProcess();
    void RestartServiceProcess(bool bIsTCP);
    string StartServiceProcess(bool bIsTCP);
}

