
using System;

#nullable disable
namespace ClassroomWindows
{
  public sealed class StudentPolicy
  {
    public string user_guid;
    public string user_type;
    public string email;
    public string first_name;
    public string last_name;
    public string username;
    public District district;
    public PolicyGroup[] groups;

    public bool IsGroupExists(string guid)
    {
      return Array.Exists<PolicyGroup>(this.groups, (Predicate<PolicyGroup>) (group => group.group_guid == guid));
    }
  }
}
