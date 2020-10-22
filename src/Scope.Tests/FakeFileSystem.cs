using Scope.Interfaces;
using Scope.Models.Interfaces;
using System.Collections.Generic;

namespace Scope.Tests
{
  internal class FakeFileSystem : IFileSystem
  {
    private List<IFile> _files = new List<IFile>();    

    public FakeFileSystem(IDirectory root)
    {
      Root = root;
    }

    public IFile this[int index] =>_files[index];

    public IDirectory Root { get; }
    public int TotalNumberOfFiles { get; }
  }

}
