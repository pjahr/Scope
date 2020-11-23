using Scope.FileViewer.DataForge.Models;
using Scope.Interfaces;
using System.ComponentModel.Composition;

namespace Scope.FileViewer.DataForge.ViewModels
{
  [Export]
  public class DataForgeFileCache
  {
    IFile _currentFile;
    DataForgeFile _current;

    public DataForgeFile this[IFile file]
    {
      get
      {
        if (_currentFile == file)
        {
          return _current;
        }
        return null;
      }
      set
      {
        _currentFile = file;
        _current = value;
      }
    }
  }
}
