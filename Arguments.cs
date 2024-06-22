
using NLog;
using System;

#nullable disable
namespace ClassroomWindows
{
  internal static class Arguments
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public static int LockSeconds = 0;
    public static bool Lock = false;
    public static string EncryptedUserEmail = (string) null;
    public static string TeacherName = (string) null;

    public static bool Parse(string[] arguments)
    {
      int num1 = 0;
      while (num1 < arguments.Length)
      {
        string[] strArray1 = arguments;
        int index1 = num1;
        int num2 = index1 + 1;
        string str1 = strArray1[index1];
        if (str1.StartsWith("/") || str1.StartsWith("-"))
        {
          if (str1.Substring(1).ToUpper() == "LOCK")
          {
            if (num2 + 2 > arguments.Length)
            {
              Arguments.logger.Error("Usage: /lock <seconds> <teacher_name>");
              return false;
            }
            string[] strArray2 = arguments;
            int index2 = num2;
            int num3 = index2 + 1;
            string s = strArray2[index2];
            try
            {
              Arguments.LockSeconds = int.Parse(s);
            }
            catch (FormatException ex)
            {
              Arguments.logger.Error("Invalid lock seconds = \"" + s + "\"");
              return false;
            }
            string[] strArray3 = arguments;
            int index3 = num3;
            num1 = index3 + 1;
            string str2 = strArray3[index3];
            Arguments.TeacherName = str2 == string.Empty ? (string) null : str2;
            Arguments.Lock = true;
          }
          else
          {
            Arguments.logger.Error("Invalid switch = \"" + str1 + "\"");
            return false;
          }
        }
        else
        {
          Arguments.logger.Error("Invalid command line argument = \"" + str1 + "\". Missing '/' or '-' switch");
          return false;
        }
      }
      return true;
    }
  }
}
