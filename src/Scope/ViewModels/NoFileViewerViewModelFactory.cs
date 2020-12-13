using Scope.Interfaces;

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

    public IFileViewer Create(IFile file)
    {
      return new NoFileViewerViewModel(file);
    }
  }
}
