using Scope.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Scope.File.SOCPAK
{
  internal class Directory : IDirectory
  {
    private readonly List<Directory> _directories;
    private readonly List<File> _files;

    public Directory(string name, string path, IEnumerable<Directory> directories = null, IEnumerable<File> files = null)
    {
      Name = name;
      Path = path;
      _directories = new List<Directory>(directories ?? new Directory[0]);
      _files = new List<File>(files ?? new File[0]);
    }
    public string Name { get; }
    public string Path { get; }
    public IReadOnlyCollection<IDirectory> Directories => _directories;
    public IReadOnlyCollection<IFile> Files => _files;
    public bool IsEmpty => !_directories.Any() && !_files.Any();

    internal void Add(Directory child)
    {
      _directories.Add(child);
    }

    internal void Add(File child)
    {
      _files.Add(child);
    }
  }
}
