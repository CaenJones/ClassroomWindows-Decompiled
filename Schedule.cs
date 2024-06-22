
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
