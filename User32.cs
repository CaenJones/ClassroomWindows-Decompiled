// Decompiled with JetBrains decompiler
// Type: ClassroomWindows.User32
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

#nullable disable
namespace ClassroomWindows
{
  public class User32
  {
    [DllImport("user32.dll")]
    private static extern bool EnumDisplaySettings(
      string deviceName,
      int modeNum,
      ref User32.DEVMODE devMode);

    [DllImport("user32.dll")]
    public static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    public static extern IntPtr GetWindowDC(IntPtr window);

    [DllImport("user32.dll")]
    public static extern IntPtr ReleaseDC(IntPtr window, IntPtr DC);

    [DllImport("user32.dll")]
    public static extern IntPtr GetWindowRect(IntPtr window, out User32.RECT rect);

    public struct RECT
    {
      public int left;
      public int top;
      public int right;
      public int bottom;
    }

    private struct DEVMODE
    {
      private const int CCHDEVICENAME = 32;
      private const int CCHFORMNAME = 32;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
      public string dmDeviceName;
      public short dmSpecVersion;
      public short dmDriverVersion;
      public short dmSize;
      public short dmDriverExtra;
      public int dmFields;
      public int dmPositionX;
      public int dmPositionY;
      public ScreenOrientation dmDisplayOrientation;
      public int dmDisplayFixedOutput;
      public short dmColor;
      public short dmDuplex;
      public short dmYResolution;
      public short dmTTOption;
      public short dmCollate;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
      public string dmFormName;
      public short dmLogPixels;
      public int dmBitsPerPel;
      public int dmPelsWidth;
      public int dmPelsHeight;
      public int dmDisplayFlags;
      public int dmDisplayFrequency;
      public int dmICMMethod;
      public int dmICMIntent;
      public int dmMediaType;
      public int dmDitherType;
      public int dmReserved1;
      public int dmReserved2;
      public int dmPanningWidth;
      public int dmPanningHeight;
    }
  }
}
