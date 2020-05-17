using Scope.Interfaces;
using Scope.Utils;

namespace Scope.ViewModels
{
  internal class NoFileViewerViewModel : IFileViewer
  {
    public NoFileViewerViewModel(IFile file)
    {
      Header = "No viewer available.";
      Text = $"There is no file viewer plugin registered for files with extension '{file.Name.GetExtension()}'.";
    }

    public string Header { get; }
    public string Text { get; }

    public void Dispose() { }
  }
}
