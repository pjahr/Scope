using System.Collections.Generic;
using Scope.Interfaces;

namespace Scope.Models.Interfaces
{
  public interface IDirectory
  {
    string Name { get; }
    IReadOnlyCollection<IDirectory> Directories { get; }
    IReadOnlyCollection<IFile> Files { get; }
  }
}
