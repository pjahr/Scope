using Scope.Interfaces;
using Scope.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

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

    public IFile? GetFile(string path)
    {
      var parts = path.Split('/');
      var directory = Root;
      for (int i = 0; i < parts.Length-1; i++)
      {
        var child = directory.Directories.SingleOrDefault(d => d.Name == parts[i]);
        if (child==null)
        {
          return null;
        }
        directory = child;
      }

      var file = directory.Files.SingleOrDefault(f => f.Name == parts.Last());
      return file;
    }
  }
}
