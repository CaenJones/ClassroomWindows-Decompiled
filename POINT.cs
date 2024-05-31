// Decompiled with JetBrains decompiler
// Type: ClassroomWindows.POINT
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

using System.Drawing;

#nullable disable
namespace ClassroomWindows
{
  public struct POINT
  {
    public int X;
    public int Y;

    public POINT(int x, int y)
    {
      this.X = x;
      this.Y = y;
    }

    public POINT(Point pt)
      : this(pt.X, pt.Y)
    {
    }

    public static implicit operator Point(POINT p) => new Point(p.X, p.Y);

    public static implicit operator POINT(Point p) => new POINT(p.X, p.Y);
  }
}
