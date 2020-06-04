using System.IO;

namespace Scope.Interfaces
{
  public interface IFile
  {
    string Name { get; }
    string Path { get; }
    long BytesCompressed { get; }
    long BytesUncompressed { get; }
    Stream Read();
  }
}
