using System;
using System.Collections.Generic;

using System.ServiceProcess;
using System.Text;

using System.Management;
using System.IO;

class ServiceManager
{
    string m_sServiceName = "";
    ServiceController m_Controller = null;
    bool m_bIsRunningTcp = false;

    public ServiceManager(string sServiceName)
    {
        m_sServiceName = sServiceName;
        m_Controller = new ServiceController(sServiceName);
    }

    ~ServiceManager()
    {
        if(m_Controller!=null)
            m_Controller.Close();
    }

    public bool IsServiceRunning()
    {
        m_Controller.Refresh();

        switch (m_Controller.Status)
        {
            case ServiceControllerStatus.Running:
            case ServiceControllerStatus.StartPending:
                return true;

            case ServiceControllerStatus.Stopped:
            case ServiceControllerStatus.StopPending:
            case ServiceControllerStatus.ContinuePending:
            case ServiceControllerStatus.PausePending:
            case ServiceControllerStatus.Paused:
                return false;
        }

        return false;
    }

    public bool IsServiceRunningInTcpMode()
    {
        if (IsServiceRunning())
        {
            return m_bIsRunningTcp;
        }
        else
        {
            return false;
        }
    }

    public string StartServiceProcess(bool bIsTCP)
    {
        try
        {
            string[] sCommands = new string[1];
            sCommands[0] = "";
            if (bIsTCP)
                sCommands[0] = "--tcp-port=443";
                
            m_Controller.Start(sCommands);
            m_bIsRunningTcp = bIsTCP;
        }
        catch (Exception Ex)
        {
            return Ex.ToString();
        }

        return "ok";
    }

    public void KillServiceProcess()
    {
        try
        {
            m_Controller.Stop();
        }
        catch (Exception Ex)
        {
        }
    }

    public void RestartServiceProcess(bool bIsTCP)
    {
        try
        {
            if (bIsTCP != m_bIsRunningTcp)
            {
                // We must restart the service to get into the other mode
                KillServiceProcess();
                StartServiceProcess(bIsTCP);
            }
            else
            {
                // Setting this guy will just tell the service to restart proxy
                m_Controller.ExecuteCommand(bIsTCP ? 130 : 129);
                m_bIsRunningTcp = bIsTCP;
            }
        }
        catch (Exception Ex)
        {

        }
    }
}
