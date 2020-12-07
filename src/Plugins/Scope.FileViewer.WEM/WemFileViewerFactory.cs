using Scope.FileViewer.WEM.Models;
using Scope.Interfaces;
using System.ComponentModel.Composition;
using System.Linq;

namespace Scope.FileViewer.WEM
{
  [Export]
  public class WemFileViewerFactory : IFileViewerFactory
  {
    private static readonly string[] Extensions;

    static WemFileViewerFactory()
    {
      Extensions = new[] { ".wem" };
    }

    public bool CanHandle(IFile file)
    {
      return Extensions.Any(e => file.Name.EndsWith(e));
    }

    public IFileViewer Create(IFile file)
    {
      return new WemFileViewer(file);
    }
  }
}
