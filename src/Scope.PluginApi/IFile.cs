using System.IO;

namespace Scope.Interfaces
{
  public interface IFile
  {
    int Index { get; }
    string Name { get; }
    string Path { get; }
    long BytesCompressed { get; }
    long BytesUncompressed { get; }
    Stream Read();
  }
}
