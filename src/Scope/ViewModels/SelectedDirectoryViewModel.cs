using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Scope.Models.Interfaces;
using Scope.Utils;
using Scope.ViewModels.Commands;

namespace Scope.ViewModels
{
  internal class SelectedDirectoryViewModel : INotifyPropertyChanged
  {
    private readonly IDirectory _directory;
    private readonly ICurrentItem _currentItem;
    private readonly ISelectedItems _selectedItems;
    private bool _isActive;
    private bool _isSelected;

    internal SelectedDirectoryViewModel(IDirectory directory,
                                        ICurrentItem currentFile,
                                        ISelectedItems selectedItems)
    {
      _directory = directory;
      _currentItem = currentFile;
      _selectedItems = selectedItems;
      SetCurrentItemCommand = new RelayCommand(() => _currentItem.ChangeTo(_directory));
      ToggleSelectionCommand = new RelayCommand(ToggleSelection);
      Items = new ObservableCollection<object>();

      HasChildren = _directory.Directories.Any() || _directory.Files.Any();

      _currentItem.Changed += SetIsActive;

      SetIsActive();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public string Name => _directory.Name;
    public string Path => _directory.Path;
    public bool HasChildren { get; private set; }
    public ObservableCollection<object> Items { get; private set; }
    public ICommand SetCurrentItemCommand { get; }
    public ICommand ToggleIsExpandedCommand { get; }
    public ICommand ToggleSelectionCommand { get; }

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

    public bool IsSelected
    {
      get { return _isSelected; }
      set
      {
        if (_isSelected == value)
        {
          return;
        }

        _isSelected = value;
        PropertyChanged.Raise(this, nameof(IsSelected));
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

    private void SetIsSelected()
    {
      IsSelected = _selectedItems.Directories.Contains(_directory);
    }

    private void ToggleSelection()
    {
      if (_isSelected)
      {
        _selectedItems.Remove(_directory);
      }
      else
      {
        _selectedItems.Add(_directory);
      }
    }
  }
}
