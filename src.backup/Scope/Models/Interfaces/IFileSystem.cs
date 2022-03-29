using Scope.Interfaces;

namespace Scope.Models.Interfaces
{
  internal interface IFileSystem
  {
    IDirectory Root { get; }
    int TotalNumberOfFiles { get; }
    IFile this[int index] { get; }
  }
}
