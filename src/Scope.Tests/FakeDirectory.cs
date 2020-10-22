using Scope.Interfaces;
using Scope.Models.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Scope.Tests
{
  internal class FakeDirectory : IDirectory
  {
    public FakeDirectory(string name, string path, IEnumerable<IDirectory> directories = null, IEnumerable<IFile> files = null)
    {
      Name = name;
      Path = path;
      Directories = directories != null ? directories.ToArray() : new IDirectory[0];
      Files = files != null ? files.ToArray() : new IFile[0];
    }

    public string Name { get; }
    public string Path { get; }
    public IReadOnlyCollection<IDirectory> Directories { get; }
    public IReadOnlyCollection<IFile> Files { get; }
    public bool IsEmpty { get; }
  }

}
