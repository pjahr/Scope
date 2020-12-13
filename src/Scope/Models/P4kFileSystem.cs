using Scope.Interfaces;
using Scope.Models.Interfaces;
using System;
using System.Collections.Generic;

namespace Scope.Models
{
  internal class P4kFileSystem : IFileSystem
  {
    private readonly IReadOnlyList<IFile> _allFiles;

    public P4kFileSystem(IDirectory root,
                         int totalNumberOfFiles,
                         IReadOnlyList<IFile> allFiles)
    {
      Root = root;
      TotalNumberOfFiles = totalNumberOfFiles;
      _allFiles = allFiles;
    }

    public IFile this[int index]
    {
      get
      {
        return _allFiles[index];
      }
    }

    public IDirectory Root { get; }
    public int TotalNumberOfFiles { get; }

    public IFile GetFile(string path)
    {
      throw new NotImplementedException();
    }
  }
}
