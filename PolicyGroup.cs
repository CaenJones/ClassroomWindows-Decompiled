// Decompiled with JetBrains decompiler
// Type: ClassroomWindows.PolicyGroup
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

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
