using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
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

    
    public async Task<IFileViewer> CreateAsync(IFile file, IProgress<ProgressReport> progress)
    {
      var df = await _provider.GetAsync(file, progress);
      return new DataForgeFileViewer(df, "");
    }
  }
}
