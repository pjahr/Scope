using Scope.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Scope.FileViewer.DataForge.Models
{
  internal class Directory : IDirectory
  {
    public Directory(string name, string path, IEnumerable<IDirectory> directories, IEnumerable<IFile> files)
    {
      Name = name;
      Path = path;
      Directories = directories.ToArray();
      Files = files.ToArray();

      IsEmpty = !directories.Any() & !files.Any();
    }
    public string Name { get; }
    public string Path { get; }
    public IReadOnlyCollection<IDirectory> Directories { get; }
    public IReadOnlyCollection<IFile> Files { get; }
    public bool IsEmpty { get; }
  }
}
