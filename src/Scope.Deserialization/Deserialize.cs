using System;
using System.IO;
using System.Linq;

namespace Scope.Deserialization
{
  public static class Deserialize
  {
    private const ByteOrderEnum Default = ByteOrderEnum.BigEndian;

    public static int ReadInt(this BinaryReader r,
                              ByteOrderEnum byteOrder = Default)
    {
      var bytes = new[] { r.ReadByte(), r.ReadByte(), r.ReadByte(), r.ReadByte() };

      if (byteOrder == ByteOrderEnum.LittleEndian)
      {
        bytes = bytes.Reverse()
                     .ToArray();
      }

      return BitConverter.ToInt32(bytes, 0);
    }

    public static short ReadInt16(this BinaryReader r,
                                  ByteOrderEnum byteOrder = Default)
    {
      var bytes = new[] { r.ReadByte(), r.ReadByte() };

      if (byteOrder == ByteOrderEnum.LittleEndian)
      {
        bytes = bytes.Reverse()
                     .ToArray();
      }

      return BitConverter.ToInt16(bytes, 0);
    }

    public static uint ReadUInt32(this BinaryReader r,
                                  ByteOrderEnum byteOrder = Default)
    {
      var bytes = new[] { r.ReadByte(), r.ReadByte(), r.ReadByte(), r.ReadByte() };

      if (byteOrder == ByteOrderEnum.LittleEndian)
      {
        bytes = bytes.Reverse()
                     .ToArray();
      }

      return BitConverter.ToUInt32(bytes, 0);
    }

    public static ushort ReadUInt16(this BinaryReader br,
                                         ByteOrderEnum byteOrder = Default)
    {
      var bytes = new byte[] {
                br.ReadByte(),
                br.ReadByte(),
            };
      if (byteOrder == ByteOrderEnum.LittleEndian)
        bytes = bytes.Reverse().ToArray();
      return BitConverter.ToUInt16(bytes, 0);
    }
  }
}
