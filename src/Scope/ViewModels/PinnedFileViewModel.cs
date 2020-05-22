using System.ComponentModel;
using System.Windows.Input;
using Scope.Interfaces;
using Scope.Models.Interfaces;
using Scope.Utils;
using Scope.ViewModels.Commands;

namespace Scope.ViewModels
{
  internal class PinnedFileViewModel
  {
    private readonly IFile _file;
    private readonly ICurrentItem _currentFile;
    private bool _isActive;

    internal PinnedFileViewModel(IFile file,
                                 ICurrentItem currentFile,
                                 IPinnedItems pinnedItems)
    {
      _file = file;
      _currentFile = currentFile;

      SetCurrentItemCommand = new RelayCommand(() => _currentFile.ChangeTo(_file));

      _currentFile.Changed += SetIsActive;

      SetIsActive();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public string Name => _file.Name;
    public string Path => _file.Path;
    public ICommand SetCurrentItemCommand { get; }

    public bool IsActive
    {
      get => _isActive;
      set
      {
        if (_isActive == value)
        {
          return;
        }

        _isActive = value;
        PropertyChanged.Raise(this, nameof(IsActive));
      }
    }

    public void Dispose()
    {
      _currentFile.Changed -= SetIsActive;
    }

    private void SetIsActive()
    {
      IsActive = _currentFile.CurrentFile == _file;
    }
  }
}
