using System.ComponentModel.Composition;
using System.Linq;
using Scope.Interfaces;

namespace Scope.FileViewer.DDS
{
  [Export]
  public class FileViewerFactory : IFileViewerFactory
  {
    private static readonly string[] Extensions;

    static FileViewerFactory()
    {
      Extensions = new[] {".dds"};
    }

    public bool CanHandle(IFile file)
    {
      return Extensions.Any(e => file.Name.EndsWith(e));
    }

    public IFileViewer Create(IFile file)
    {
      return new DdsFileViewer(file);
    }
  }
}
