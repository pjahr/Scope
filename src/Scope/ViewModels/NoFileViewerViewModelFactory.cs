using Scope.Interfaces;
using System;
using System.Threading.Tasks;

namespace Scope.ViewModels
{
  /// <summary>
  /// Creates a null-object rendered by a default view to inform the user that there is no
  /// plugin file viewer is available.
  /// </summary>
  internal class NoFileViewerViewModelFactory : IFileViewerFactory
  {
    public bool CanHandle(IFile file)
    {
      return true;
    }

    public FileCategory Category => FileCategory.Other;    

    public Task<IFileViewer> CreateAsync(IFile file, IProgress<ProgressReport> progress)
    {
      return Task.FromResult((IFileViewer)new NoFileViewerViewModel(file));
    }
  }
}
