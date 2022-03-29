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
    private readonly ICurrentItem _currentFile;
    private bool _isActive;

    internal PinnedFileViewModel(IFile file,
                                 ICurrentItem currentFile,
                                 IPinnedItems pinnedItems)
    {
      Model = file;
      _currentFile = currentFile;

      SetCurrentItemCommand = new RelayCommand(() => _currentFile.ChangeTo(Model));

      _currentFile.Changed += SetIsActive;

      SetIsActive();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public IFile Model { get; }
    public string Name => Model.Name;
    public string Path => Model.Path;
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
      IsActive = _currentFile.CurrentFile == Model;
    }
  }
}
