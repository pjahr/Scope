using System;
using Scope.Interfaces;

namespace Scope.ViewModels
{
  internal class FileItemViewModel : IDisposable
  {
    private readonly IFile _file;

    public FileItemViewModel(IFile currentFile, IFileViewerFactory factory)
    {
      _file = currentFile;

      Header = _file.Name;
      Path = _file.Path;

      //Viewer = factory.CreateAsync(_file, new Progress<ProgressReport>());
    }

    public string Header { get; }
    public string Path { get; }

    public IFileViewer Viewer { get; }

    public void Dispose()
    {
      Viewer?.Dispose();
    }
  }
}
