using System.IO;

namespace Scope.Deserialization
{
  public static class BinaryReaderExtensions
  {
    /// <summary>
    /// Read a NULL-Terminated string from the stream
    /// </summary>
    public static string ReadCString(this BinaryReader binaryReader)
    {
      int stringLength = 0;

      while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length
             && binaryReader.ReadChar() != 0)
      {
        stringLength++;
      }

      long nul = binaryReader.BaseStream.Position;

      binaryReader.BaseStream.Seek(0 - stringLength - 1, SeekOrigin.Current);

      char[] chars = binaryReader.ReadChars(stringLength + 1);

      binaryReader.BaseStream.Seek(nul, SeekOrigin.Begin);

      // Why is this necessary?
      if (stringLength > chars.Length)
      {
        stringLength = chars.Length;
      }

      // If there is actually a string to read
      if (stringLength > 0)
      {
        return new string(chars, 0, stringLength).Replace("\u0000", "");
      }

      return null;
    }

    /// <summary>
    /// Read a Fixed-Length string from the stream
    /// </summary>
    public static string ReadFString(this BinaryReader binaryReader, int stringLength)
    {
      char[] chars = binaryReader.ReadChars(stringLength);

      for (int i = 0; i < stringLength; i++)
      {
        if (chars[i] == 0)
        {
          return new string(chars, 0, i);
        }
      }

      return new string(chars);
    }
  }
}
