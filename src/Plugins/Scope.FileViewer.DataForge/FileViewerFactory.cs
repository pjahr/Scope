using System.ComponentModel.Composition;
using System.Linq;
using Scope.Interfaces;

namespace Scope.FileViewer.DataForge
{
  [Export]
  public class FileViewerFactory : IFileViewerFactory
  {
    private static readonly string[] _extensions;

    static FileViewerFactory()
    {
      _extensions = new[] {".dcb"};
    }

    public bool CanHandle(IFile file)
    {
      return _extensions.Any(e => file.Name.EndsWith(e));
    }

    public IFileViewer Create(IFile file)
    {
      return new DataForgeFileViewer(file);
    }
  }
}
