using System.ComponentModel;
using System.Windows.Input;
using Scope.Interfaces;
using Scope.Models.Interfaces;
using Scope.Utils;
using Scope.ViewModels.Commands;

namespace Scope.ViewModels
{
  internal class PinnedDirectoryViewModel : INotifyPropertyChanged
  {
    private readonly ICurrentItem _currentItem;
    private bool _isActive;

    internal PinnedDirectoryViewModel(IDirectory directory,
                                      ICurrentItem currentItem,
                                      IPinnedItems selectedItems)
    {
      Model = directory;

      _currentItem = currentItem;
      
      SetCurrentItemCommand = new RelayCommand(() => _currentItem.ChangeTo(Model));
            
      _currentItem.Changed += SetIsActive;

      SetIsActive();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public IDirectory Model { get; }
    public string Name => Model.Name;
    public string Path => Model.Path;

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
      IsActive = _currentItem.CurrentDirectory == Model;
    }
  }
}
