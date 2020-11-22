using System.Collections.Generic;

namespace Scope.Interfaces
{
  public interface IDirectory
  {
    string Name { get; }
    string Path { get; }
    IReadOnlyCollection<IDirectory> Directories { get; }
    IReadOnlyCollection<IFile> Files { get; }

    bool IsEmpty { get; }
  }
}
