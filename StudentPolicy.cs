// Decompiled with JetBrains decompiler
// Type: ClassroomWindows.StudentPolicy
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

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
