// Decompiled with JetBrains decompiler
// Type: ClassroomWindows.Site
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

using System;
using System.Linq;

#nullable disable
namespace ClassroomWindows
{
  public sealed class Site
  {
    public string URI { get; private set; }

    public string Host { get; private set; }

    public string PathAndQuery { get; private set; }

    public Site(string site)
    {
      if (site.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || site.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
      {
        this.URI = site.TrimEnd('/');
      }
      else
      {
        char[] anyOf = new char[2]{ '/', '?' };
        int num;
        if ((num = site.IndexOfAny(anyOf)) != -1)
        {
          this.Host = site.Substring(0, num);
          this.PathAndQuery = site.Substring(num).TrimEnd('/');
        }
        else
          this.Host = site;
      }
    }

    public bool Match(Uri uri)
    {
      if (this.URI != null)
      {
        if (!uri.AbsoluteUri.StartsWith(this.URI, StringComparison.OrdinalIgnoreCase))
          return false;
        return uri.AbsoluteUri.Length <= this.URI.Length || "/?".Contains<char>(uri.AbsoluteUri[this.URI.Length]);
      }
      if (!uri.Host.EndsWith(this.Host, StringComparison.OrdinalIgnoreCase) || uri.Host.Length > this.Host.Length && uri.Host[uri.Host.Length - this.Host.Length - 1] != '.')
        return false;
      if (this.PathAndQuery == null)
        return true;
      if (!uri.PathAndQuery.StartsWith(this.PathAndQuery, StringComparison.OrdinalIgnoreCase))
        return false;
      return uri.PathAndQuery.Length <= this.PathAndQuery.Length || "/?&".Contains<char>(uri.PathAndQuery[this.PathAndQuery.Length]);
    }
  }
}
