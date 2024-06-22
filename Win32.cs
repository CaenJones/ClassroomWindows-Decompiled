
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

#nullable disable
namespace ClassroomWindows
{
  public static class Win32
  {
    public const ulong HResultError = 2147483648;
    public const int MaxPathLength = 260;

    public static bool IsHresultError(ulong result) => ((long) result & 2147483648L) == 0L;

    public static bool IsHresultSuccess(ulong result) => !Win32.IsHresultError(result);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SendMessage(
      HandleRef hWnd,
      uint Msg,
      IntPtr wParam,
      IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern IntPtr WindowFromPoint(POINT p);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetCursorPos(ref POINT p);

    [DllImport("user32.dll")]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("Iphlpapi.dll", SetLastError = true)]
    public static extern uint NotifyAddrChange(ref IntPtr Handle, ref NativeOverlapped overlapped);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr LoadLibrary(string dllToLoad);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

    [DllImport("kernel32.dll")]
    public static extern bool FreeLibrary(IntPtr hModule);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool QueryFullProcessImageName(
      IntPtr process,
      uint flags,
      [MarshalAs(UnmanagedType.LPTStr), Out] StringBuilder processPath,
      ref uint size);

    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(
      Win32.ProcessAccess desiredAccess,
      [MarshalAs(UnmanagedType.Bool)] bool inheritHandle,
      uint processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CloseHandle(IntPtr handle);

    [DllImport("SHCore.dll")]
    public static extern ulong SetProcessDpiAwareness(Win32.PROCESS_DPI_AWARENESS awareness);

    public static bool SetDpiAware()
    {
      return Win32.IsHresultSuccess(Win32.SetProcessDpiAwareness(Win32.PROCESS_DPI_AWARENESS.Process_System_DPI_Aware));
    }

    [DllImport("user32.dll")]
    public static extern int GetSystemMetrics(Win32.SystemMetric index);

    public static void GetVirtualScreenSize(out int width, out int height)
    {
      width = Win32.GetSystemMetrics(Win32.SystemMetric.CxVirtualScreen);
      height = Win32.GetSystemMetrics(Win32.SystemMetric.CyVirtualScreen);
    }

    [DllImport("gdi32.dll")]
    private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    public static void GetDesktopSize(out int width, out int height)
    {
      IntPtr hdc = Graphics.FromHwnd(IntPtr.Zero).GetHdc();
      height = Win32.GetDeviceCaps(hdc, 117);
      width = Win32.GetDeviceCaps(hdc, 118);
    }

    [Flags]
    public enum ProcessAccess : uint
    {
      allAccess = 2035711, // 0x001F0FFF
      terminate = 1,
      createThread = 2,
      vmOperation = 8,
      vmRead = 16, // 0x00000010
      vmWrite = 32, // 0x00000020
      dupHandle = 64, // 0x00000040
      setInformation = 512, // 0x00000200
      setQuota = 256, // 0x00000100
      queryInformation = 1024, // 0x00000400
      queryLimitedInformation = 4096, // 0x00001000
      createProcess = 128, // 0x00000080
      suspendResume = 2048, // 0x00000800
      synchronize = 1048576, // 0x00100000
    }

    public enum Error
    {
      Success = 0,
      AccessDenied = 5,
      ProcNotFound = 127, // 0x0000007F
    }

    public enum PROCESS_DPI_AWARENESS
    {
      Process_DPI_Unaware,
      Process_System_DPI_Aware,
      Process_Per_Monitor_DPI_Aware,
    }

    public enum SystemMetric
    {
      CxVirtualScreen = 78, // 0x0000004E
      CyVirtualScreen = 79, // 0x0000004F
    }

    private enum DeviceCap
    {
      Desktopvertres = 117, // 0x00000075
      Desktophorzres = 118, // 0x00000076
    }
  }
}
