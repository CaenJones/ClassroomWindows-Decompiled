
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
