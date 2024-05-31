// Decompiled with JetBrains decompiler
// Type: ClassroomWindows.Global
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

#nullable disable
namespace ClassroomWindows
{
  internal static class Global
  {
    public const string apiReport = "/report";
    public const string apiPolicy = "/policy";
    public const string apiIp = "/ip";
    public static DateTime UnixEpoch = new DateTime(1970, 1, 1);
    public static string versionHeader = "windows-" + Global.GetAppVersion();
    public const int MsecsPerSec = 1000;
    public static string newLine = "\n    ";

    public static string GetAppVersion()
    {
      return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
    }

    public static string IndentLines(string source)
    {
      return source.TrimEnd('\n').Replace("\n", Global.newLine);
    }

    public static string RemoveLineEndings(string source)
    {
      return Regex.Replace(source, "\\r\\n?|\\n", "");
    }

    public static string PrettyJson(string json)
    {
      return JToken.Parse(json).ToString((Formatting) 1, Array.Empty<JsonConverter>());
    }

    private static string HexPointer(IntPtr pointer)
    {
      return string.Format("0x{0:x}", (object) pointer.ToInt64());
    }

    public static string Win32ErrorMessage()
    {
      int lastWin32Error = Marshal.GetLastWin32Error();
      return string.Format("{0}. Error code = {1}.", (object) new Win32Exception(lastWin32Error).Message.Trim('.'), (object) lastWin32Error);
    }

    public static string SingularPlural(int number, string word)
    {
      return number != 1 ? string.Format("{0} {1}s", (object) number, (object) word) : string.Format("{0} {1}", (object) number, (object) word);
    }

    public static ulong GetUnixTimeStamp()
    {
      return (ulong) DateTime.UtcNow.Subtract(Global.UnixEpoch).TotalMilliseconds;
    }

    public static string HttpStatusText(HttpStatusCode status)
    {
      string str1 = status.ToString();
      string str2 = ((int) status).ToString();
      if (str1.Equals(str2))
        return "HTTP status = " + str1;
      return "HTTP status = " + str1 + " (" + str2 + ")";
    }
  }
}
