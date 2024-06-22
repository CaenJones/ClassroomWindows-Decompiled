
using System.Collections.Generic;

#nullable disable
namespace ClassroomWindows
{
  public sealed class PolicyGroup
  {
    public string name;
    public Dictionary<string, Schedule> class_schedule;
    public string group_guid;
    public WebRules web_rules;
  }
}
