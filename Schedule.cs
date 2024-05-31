// Decompiled with JetBrains decompiler
// Type: ClassroomWindows.Schedule
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

#nullable disable
namespace ClassroomWindows
{
  public sealed class Schedule
  {
    public float? s;
    public float? e;

    public bool HasValues => this.s.HasValue && this.e.HasValue;
  }
}
