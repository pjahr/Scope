using System.Collections.Generic;
using Scope.Interfaces;

namespace Scope.Models.Interfaces
{
  public interface IDirectory
  {
    string Name { get; }
    string Path { get; }
    IReadOnlyCollection<IDirectory> Directories { get; }
    IReadOnlyCollection<IFile> Files { get; }
  }
}
