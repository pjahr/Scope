using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Scope.FileViewer.DataForge
{
  public static class Extensions
  {

    /// <summary>
    /// Read a NULL-Terminated string from the stream
    /// </summary>
    /// <param name="binaryReader"></param>
    /// <returns></returns>
    public static String ReadNullTerminatedString(this BinaryReader binaryReader)
    {
      Int32 stringLength = 0;      

      while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length && Convert.ToChar(binaryReader.ReadByte()) != 0)
        stringLength++;

      Int64 nul = binaryReader.BaseStream.Position;

      binaryReader.BaseStream.Seek(0 - stringLength - 1, SeekOrigin.Current);

      Char[] chars = Enumerable.Range(0, stringLength + 1).Select(_=>Convert.ToChar(binaryReader.ReadByte())).ToArray();

      binaryReader.BaseStream.Seek(nul, SeekOrigin.Begin);

      // Why is this necessary?
      if (stringLength > chars.Length) stringLength = chars.Length;

      // If there is actually a string to read
      if (stringLength > 0)
      {
        return new String(chars, 0, stringLength).Replace("\u0000", "");
      }

      return null;
    }

    public static Guid ReadGuid(this BinaryReader reader)
    {
      var c = reader.ReadInt16();
      var b = reader.ReadInt16();
      var a = reader.ReadInt32();
      var k = reader.ReadByte();
      var j = reader.ReadByte();
      var i = reader.ReadByte();
      var h = reader.ReadByte();
      var g = reader.ReadByte();
      var f = reader.ReadByte();
      var e = reader.ReadByte();
      var d = reader.ReadByte();

      return new Guid(a, b, c, d, e, f, g, h, i, j, k);
    }

    public static void Times(this int n, Action action)
    {
      for (int i = 0; i < n; i++)
      {
        action();
      }
    }

    public static IEnumerable<T> Times<T>(this int n, Func<T> generate)
    {
      for (int i = 0; i < n; i++)
      {
        yield return generate();
      }
    }

    public static IEnumerable<T> Times<T>(this int n, Func<int,T> generate)
    {
      for (int i = 0; i < n; i++)
      {
        yield return generate(i);
      }
    }

    public static T[] ToArray<T>(this int n, Func<T> generate)
    {
      return n.Times(generate).ToArray();
    }
  }
}
