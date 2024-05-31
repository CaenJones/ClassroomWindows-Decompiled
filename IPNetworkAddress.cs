// Decompiled with JetBrains decompiler
// Type: ClassroomWindows.IPNetworkAddress
// Assembly: ClassroomWindows, Version=3.4.7.6, Culture=neutral, PublicKeyToken=null
// MVID: 59349F94-FAF6-4BCE-A4AE-E4F9DD746CAB
// Assembly location: C:\Users\Karim\Downloads\ClassroomWindows.exe

using System.Net;

#nullable disable
namespace ClassroomWindows
{
  internal class IPNetworkAddress
  {
    public IPAddress ipAddress;
    public byte[] octets;
    public byte[] mask;
    public int cidr;
    public int size;

    public IPNetworkAddress()
    {
      this.cidr = 0;
      this.size = 0;
      this.ipAddress = (IPAddress) null;
      this.octets = (byte[]) null;
      this.mask = (byte[]) null;
    }

    private void Init()
    {
      this.octets = this.ipAddress.GetAddressBytes();
      this.size = this.octets.GetLength(0);
      this.mask = IPNetworkAddress.GetNetworkMask(this.size, this.cidr);
    }

    public static bool TryParse(string ipAddressText, out IPNetworkAddress networkAddress)
    {
      networkAddress = new IPNetworkAddress();
      int length = ipAddressText.LastIndexOf('/');
      if (length < 0)
      {
        if (!IPAddress.TryParse(ipAddressText, out networkAddress.ipAddress))
          return false;
        networkAddress.Init();
        return true;
      }
      string s = ipAddressText.Substring(length + 1);
      ipAddressText = ipAddressText.Substring(0, length);
      if (!IPAddress.TryParse(ipAddressText, out networkAddress.ipAddress) || !int.TryParse(s, out networkAddress.cidr) || networkAddress.cidr < 0 || networkAddress.cidr > networkAddress.ipAddress.GetAddressBytes().GetLength(0) * 8)
        return false;
      networkAddress.Init();
      return true;
    }

    public override string ToString()
    {
      return string.Format("{0}/{1}", (object) this.ipAddress, (object) this.cidr);
    }

    public bool IsInNetwork(IPAddress ipAddress)
    {
      byte[] addressBytes = ipAddress.GetAddressBytes();
      if (this.size != addressBytes.GetLength(0))
        return false;
      for (int index = 0; index < this.size; ++index)
      {
        uint num = (uint) this.mask[index];
        if (((int) this.octets[index] & (int) num) != ((int) addressBytes[index] & (int) num))
          return false;
      }
      return true;
    }

    public static byte[] GetNetworkMask(int size, int cidr)
    {
      byte[] networkMask = new byte[size];
      cidr = size * 8 - cidr;
      for (int index = networkMask.GetLength(0) - 1; index >= 0; --index)
      {
        if (cidr >= 8)
        {
          networkMask[index] = (byte) 0;
          cidr -= 8;
        }
        else if (cidr > 0)
        {
          networkMask[index] = (byte) ~((1 << cidr) - 1);
          cidr = 0;
        }
        else
          networkMask[index] = byte.MaxValue;
      }
      return networkMask;
    }
  }
}
