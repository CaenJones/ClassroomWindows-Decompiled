// Decompiled with JetBrains decompiler
// Type: Encryption_XOR.EncryptorDecryptor
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

using System.Text;

#nullable disable
namespace Encryption_XOR
{
  public static class EncryptorDecryptor
  {
    private static string _TableCheck = "acegikmo0988Zk";
    private static int _KeyIntChecker = 199;

    public static string EncoderOne(string inputString)
    {
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < inputString.Length; ++index)
        stringBuilder.Append((char) ((uint) inputString[index] ^ (uint) EncryptorDecryptor._TableCheck[index % EncryptorDecryptor._TableCheck.Length]));
      return stringBuilder.ToString();
    }

    public static string EncoderTwo(string sInputData)
    {
      StringBuilder stringBuilder1 = new StringBuilder(sInputData);
      StringBuilder stringBuilder2 = new StringBuilder(sInputData.Length);
      for (int index = 0; index < sInputData.Length; ++index)
      {
        char ch = (char) ((uint) stringBuilder1[index] ^ (uint) EncryptorDecryptor._KeyIntChecker);
        stringBuilder2.Append(ch);
      }
      return stringBuilder2.ToString();
    }
  }
}
