using Scope.Interfaces;
using System.IO;

namespace Scope.File.SOCPAK
{
  public class File : IFile
  {
    private readonly byte[] _bytes;

    public File(string name, string path, byte[] bytes, int bytesCompressed)
    {
      Name = name;
      Path = path;
      _bytes = bytes;
      BytesCompressed = bytesCompressed;
      BytesUncompressed = _bytes.Length;
    }

    public int Index { get; }
    public string Name { get; }
    public string Path { get; }
    public long BytesCompressed { get; }
    public long BytesUncompressed { get; }

    public Stream Read()
    {
      return new MemoryStream(_bytes);
    }
  }
}
