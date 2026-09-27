using System;
using System.Diagnostics;
using ShutUp11.Native;

namespace ShutUp11.Services
{
    public class KillResult
    {
        public int Pid { get; set; }
        public bool Success { get; set; }
        public string? Method { get; set; }
        public string? Error { get; set; }
    }

    public static class ProcessKiller
    {
        private static bool _debugPrivilegeAcquired;

        public static KillResult Kill(int pid)
        {
            var result = new KillResult { Pid = pid };

            if (pid == 0 || pid == 4)
            {
                result.Error = "System process cannot be terminated.";
                return result;
            }

            Process? p = null;
            try { p = Process.GetProcessById(pid); }
            catch (ArgumentException)
            {
                result.Success = true;
                result.Method = "Already exited";
                return result;
            }

            try
            {
                try
                {
                    if (p.CloseMainWindow() && p.WaitForExit(800))
                    {
                        result.Success = true;
                        result.Method = "Graceful (WM_CLOSE)";
                        return result;
                    }
                }
                catch { }

                try
                {
                    p.Kill(false);
                    if (p.WaitForExit(1500))
                    {
                        result.Success = true;
                        result.Method = "Process.Kill";
                        return result;
                    }
                }
                catch { }

                try
                {
                    p.Kill(true);
                    if (p.WaitForExit(2000))
                    {
                        result.Success = true;
                        result.Method = "Process.Kill (tree)";
                        return result;
                    }
                }
                catch { }

                AcquireDebugPrivilege();

                IntPtr handle = NativeMethods.OpenProcess(
                    NativeMethods.PROCESS_TERMINATE | NativeMethods.PROCESS_QUERY_LIMITED_INFORMATION,
                    false, pid);

                if (handle != IntPtr.Zero)
                {
                    try
                    {
                        int status = NativeMethods.NtTerminateProcess(handle, 0);
                        if (status == 0)
                        {
                            result.Success = true;
                            result.Method = "NtTerminateProcess (native)";
                            return result;
                        }
                    }
                    finally { NativeMethods.CloseHandle(handle); }
                }

                System.Threading.Thread.Sleep(300);
                try
                {
                    Process.GetProcessById(pid);
                    result.Error = "All kill layers failed. Process is protected (AV/EDR).";
                }
                catch (ArgumentException)
                {
                    result.Success = true;
                    result.Method = "Terminated (verified)";
                }
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }

            return result;
        }

        private static void AcquireDebugPrivilege()
        {
            if (_debugPrivilegeAcquired) return;

            try
            {
                if (!NativeMethods.OpenProcessToken(
                    Process.GetCurrentProcess().Handle,
                    NativeMethods.TOKEN_ADJUST_PRIVILEGES | NativeMethods.TOKEN_QUERY,
                    out IntPtr token)) return;

                try
                {
                    if (!NativeMethods.LookupPrivilegeValue(null, "SeDebugPrivilege", out var luid))
                        return;

                    var tp = new NativeMethods.TOKEN_PRIVILEGES
                    {
                        PrivilegeCount = 1,
                        Privileges = new NativeMethods.LUID_AND_ATTRIBUTES
                        {
                            Luid = luid,
                            Attributes = NativeMethods.SE_PRIVILEGE_ENABLED
                        }
                    };

                    NativeMethods.AdjustTokenPrivileges(token, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
                    _debugPrivilegeAcquired = true;
                }
                finally { NativeMethods.CloseHandle(token); }
            }
            catch { }
        }
    }
}