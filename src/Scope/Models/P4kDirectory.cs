using System.Collections.Generic;
using Scope.Interfaces;
using Scope.Models.Interfaces;

namespace Scope.Models
{
  internal class P4kDirectory : IDirectory
  {
    private readonly List<IDirectory> _directories = new List<IDirectory>();
    private readonly List<IFile> _files = new List<IFile>();

    private P4kDirectory()
    {
      Name = "Root";
      Path = string.Empty;
    }

    public P4kDirectory(string name, string path)
    {
      Name = name;
      Path = path;
    }

    public string Name { get; }
    public string Path { get; }
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
