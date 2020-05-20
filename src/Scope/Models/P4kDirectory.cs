using System.Collections.Generic;
using Scope.Interfaces;
using Scope.Models.Interfaces;

namespace Scope.Models
{
  internal class P4kDirectory : IDirectory
  {
    private readonly List<IDirectory> _directories = new List<IDirectory>();
    private readonly List<IFile> _files = new List<IFile>();
    private readonly IDirectory _parent;

    private P4kDirectory()
    {
      Name = "Root";
    }

    public P4kDirectory(string name)
    {
      Name = name;
    }

    public string Name { get; }
    public IReadOnlyCollection<IDirectory> Directories => _directories;
    public IReadOnlyCollection<IFile> Files => _files;

    internal void Add(IDirectory child)
    {
      _directories.Add(child);
    }

    internal void Add(IFile child)
    {
      _files.Add(child);
    }

    internal static P4kDirectory Root()
    {
      return new P4kDirectory();
    }
  }
}
