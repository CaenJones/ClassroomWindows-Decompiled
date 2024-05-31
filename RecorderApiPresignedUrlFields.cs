// Decompiled with JetBrains decompiler
// Type: ClassroomWindows.RecorderApiPresignedUrlFields
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

using Newtonsoft.Json;

#nullable disable
namespace ClassroomWindows
{
  public sealed class RecorderApiPresignedUrlFields
  {
    public string bucket;
    [JsonProperty("X-Amz-Algorithm")]
    public string XAmzAlgorithm;
    [JsonProperty("X-Amz-Credential")]
    public string XAmzCredential;
    [JsonProperty("X-Amz-Date")]
    public string XAmzDate;
    [JsonProperty("X-Amz-Security-Token")]
    public string XAmzSecurityToken;
    public string Policy;
    [JsonProperty("X-Amz-Signature")]
    public string XAmzSignature;
  }
}
