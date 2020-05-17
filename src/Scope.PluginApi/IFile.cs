using System.IO;

namespace Scope.Interfaces
{
  public interface IFile
  {
    string Name { get; }
    string Path { get; }
    Stream Read();
  }
}
