using System.ComponentModel.Composition;
using System.Linq;
using Scope.FileViewer.DataForge.ViewModels;
using Scope.Interfaces;

namespace Scope.FileViewer.DataForge
{
  [Export]
  internal class DataForgeFileViewerFactory : IFileViewerFactory
  {
    private static readonly string[] Extensions = { ".dcb" };
    private readonly DataForgeFileProvider _provider;

    public DataForgeFileViewerFactory(DataForgeFileProvider provider)
    {
      _provider = provider;
    }

    public FileCategory Category => FileCategory.Container;

    public bool CanHandle(IFile file)
    {
      return Extensions.Any(e => file.Name.EndsWith(e));
    }

    public IFileViewer Create(IFile file)
    {
      var df = _provider.Get(file, out string errorMessage);
      return new DataForgeFileViewer(df, errorMessage);
    }
  }
}
