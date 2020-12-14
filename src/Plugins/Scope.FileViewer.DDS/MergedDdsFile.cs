using Scope.Interfaces;
using System.IO;

namespace Scope.FileViewer.DDS
{
  class MergedDdsFile : IFile
  {
    private readonly IFile _file;
    private readonly IFile _baseFile;

    public MergedDdsFile(IFile file, IFile baseFile)
    {
      _file = file;
      _baseFile = baseFile;
    }

    public int Index => _file.Index;
    public string Name => _file.Name;
    public string Path => _file.Path;
    public long BytesCompressed => _file.BytesCompressed;
    public long BytesUncompressed => _file.BytesUncompressed;


    public Stream Read()
    {
      return new ConcatenatedStream(_baseFile.Read(), _file.Read());
    }
  }
}
