using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using ShutUp11.Models;

namespace ShutUp11.Services
{
    public static class ServiceProvider
    {
        public static List<ServiceItem> GetAll()
        {
            var result = new List<ServiceItem>();
            foreach (var sc in ServiceController.GetServices().OrderBy(s => s.DisplayName))
            {
                bool running = false;
                string startType = "";
                try
                {
                    running = sc.Status == ServiceControllerStatus.Running;
                    startType = sc.StartType.ToString();
                }
                catch { }

                bool sendsData = sc.ServiceName is "DiagTrack" or "dmwappushservice"
                    or "WerSvc" or "PcaSvc" or "DoSvc" or "wuauserv"
                    or "XblAuthManager" or "XblGameSave" or "XboxNetApiSvc"
                    or "MapsBroker" or "RetailDemo" or "WSearch";

                result.Add(new ServiceItem
                {
                    Name = sc.ServiceName,
                    DisplayName = sc.DisplayName,
                    IsRunning = running,
                    StartType = startType,
                    SendsData = sendsData,
                    Tag = sc.ServiceName
                });
            }
            return result;
        }

        public static bool SetRunning(string name, bool run)
        {
            try
            {
                using var sc = new ServiceController(name);
                if (run && sc.Status != ServiceControllerStatus.Running)
                {
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                }
                else if (!run && sc.Status != ServiceControllerStatus.Stopped)
                {
                    if (sc.CanStop)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                    }
                }
                return true;
            }
            catch { return false; }
        }

        public static bool SetStartType(string name, ServiceStartMode mode)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    $"SELECT * FROM Win32_Service WHERE Name='{name}'");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string startMode = mode switch
                    {
                        ServiceStartMode.Automatic => "Automatic",
                        ServiceStartMode.Manual => "Manual",
                        ServiceStartMode.Disabled => "Disabled",
                        _ => "Manual"
                    };
                    obj.InvokeMethod("ChangeStartMode", new object[] { startMode });
                    return true;
                }
                return false;
            }
            catch { return false; }
        }
    }
}