
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
