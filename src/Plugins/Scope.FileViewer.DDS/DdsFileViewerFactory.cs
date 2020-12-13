using System.ComponentModel.Composition;
using System.Linq;
using Scope.Interfaces;

namespace Scope.FileViewer.DDS
{
  [Export]
  public class DdsFileViewerFactory : IFileViewerFactory
  {
    private static readonly string[] Extensions;

    static DdsFileViewerFactory()
    {
      Extensions = new[] {".dds"};
    }

    public FileCategory Category => FileCategory.Image;

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
