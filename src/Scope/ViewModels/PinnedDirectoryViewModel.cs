using System.ComponentModel;
using System.Windows.Input;
using Scope.Models.Interfaces;
using Scope.Utils;
using Scope.ViewModels.Commands;

namespace Scope.ViewModels
{
  internal class PinnedDirectoryViewModel : INotifyPropertyChanged
  {
    private readonly IDirectory _directory;
    private readonly ICurrentItem _currentItem;
    private bool _isActive;

    internal PinnedDirectoryViewModel(IDirectory directory,
                                      ICurrentItem currentFile,
                                      IPinnedItems selectedItems)
    {
      _directory = directory;
      _currentItem = currentFile;
      
      SetCurrentItemCommand = new RelayCommand(() => _currentItem.ChangeTo(_directory));
            
      _currentItem.Changed += SetIsActive;

      SetIsActive();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public string Name => _directory.Name;
    public string Path => _directory.Path;
    
    public ICommand SetCurrentItemCommand { get; }

    public bool IsActive
    {
      get { return _isActive; }
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
      _currentItem.Changed -= SetIsActive;
    }

    private void SetIsActive()
    {
      IsActive = _currentItem.CurrentDirectory == _directory;
    }
  }
}
