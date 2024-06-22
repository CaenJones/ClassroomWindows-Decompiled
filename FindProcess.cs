
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

#nullable disable
namespace ClassroomWindows
{
  public class FindProcess
  {
    private static FindProcess.GetProcessPathDelegate GetProcessPath;

    static FindProcess()
    {
      if (Environment.OSVersion.Version.Major >= 6)
        FindProcess.GetProcessPath = new FindProcess.GetProcessPathDelegate(FindProcess.GetProcessPath_Win32);
      else
        FindProcess.GetProcessPath = new FindProcess.GetProcessPathDelegate(FindProcess.GetProcessPath_CLR);
    }

    public static bool IsRunning(string findProcessName)
    {
      foreach (Process process in Process.GetProcesses())
      {
        string path;
        if (process.SessionId == Process.GetCurrentProcess().SessionId && FindProcess.GetProcessPath(process, out path) && string.Equals(Path.GetFileName(path), findProcessName, StringComparison.OrdinalIgnoreCase))
          return true;
      }
      return false;
    }

    public static bool GetProcessPath_CLR(Process process, out string path)
    {
      try
      {
        path = process.MainModule.FileName;
        return true;
      }
      catch
      {
        path = (string) null;
        return false;
      }
    }

    public static bool GetProcessPath_Win32(Process process, out string path)
    {
      StringBuilder processPath = new StringBuilder(261);
      IntPtr num = Win32.OpenProcess(Win32.ProcessAccess.queryLimitedInformation, false, (uint) process.Id);
      path = (string) null;
      bool processPathWin32 = false;
      if (num != IntPtr.Zero)
      {
        uint capacity = (uint) processPath.Capacity;
        if (Win32.QueryFullProcessImageName(num, 0U, processPath, ref capacity))
        {
          path = processPath.ToString();
          processPathWin32 = true;
        }
        Win32.CloseHandle(num);
      }
      return processPathWin32;
    }

    public delegate bool GetProcessPathDelegate(Process process, out string path);
  }
}
