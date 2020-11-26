using Scope.Interfaces;
using System.IO;

namespace Scope.FileViewer.DataForge.Models
{
  internal class File : IFile
  {
    private readonly byte[] _bytes;
    private readonly Struct _dataForgeItem;

    public File(string name, string path, Struct dataForgeItem)
    {
      Name = name;
      Path = path;
      _dataForgeItem = dataForgeItem;
      _bytes = new byte[0];
      BytesUncompressed = _bytes.Length;
      BytesCompressed = _bytes.Length;
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
