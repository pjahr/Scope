using System;
using System.Threading.Tasks;
using Nito.Mvvm;
using Scope.Interfaces;

namespace Scope.ViewModels
{
  internal class FileItemViewModel : IDisposable
  {
    private readonly IFile _file;
    private readonly IFileViewerFactory _factory;
    private IFileViewer _fileViewer;

    public FileItemViewModel(IFile currentFile, IFileViewerFactory factory)
    {
      _file = currentFile;
      _factory = factory;

      Header = _file.Name;
      Path = _file.Path;

      Viewer = NotifyTask.Create(GetFileViewer());
    }

    public string Header { get; }
    public string Path { get; }

    public NotifyTask<IFileViewer> Viewer { get; }

    private async Task<IFileViewer> GetFileViewer()
    {
      if (_fileViewer != null)
      {
        return _fileViewer;
      }

      _fileViewer = await _factory.CreateAsync(_file, new Progress<ProgressReport>());

      if (_fileViewer != null)
      {
        return _fileViewer;

      }

      return new NoFileViewerViewModel(_file);
    }

    public void Dispose()
    {
      _fileViewer?.Dispose();
    }
  }
}
