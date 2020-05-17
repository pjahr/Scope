using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Scope.Interfaces;
using Scope.Models.Interfaces;
using Scope.Utils;
using Scope.ViewModels.Commands;

namespace Scope.ViewModels
{
  internal class SelectedFileViewModel
  {
    private readonly IFile _file;
    private readonly ICurrentItem _currentFile;
    private readonly ISelectedItems _selectedItems;
    private bool _isActive;
    private bool _isSelected;

    internal SelectedFileViewModel(IFile file,
                         ICurrentItem currentFile,
                         ISelectedItems selectedItems)
    {
      _file = file;
      _currentFile = currentFile;
      _selectedItems = selectedItems;

      SetCurrentItemCommand = new RelayCommand(() => _currentFile.ChangeTo(_file));
      ToggleSelectionCommand = new RelayCommand(ToggleSelection);

      _currentFile.Changed += SetIsActive;
      _selectedItems.Changed += SetIsSelected;

      SetIsActive();
      SetIsSelected();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public string Name => _file.Name;
    public ICommand SetCurrentItemCommand { get; }
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
      _currentFile.Changed -= SetIsActive;
    }

    private void SetIsActive()
    {
      IsActive = _currentFile.CurrentFile == _file;
    }

    private void SetIsSelected()
    {
      IsSelected = _selectedItems.Files.Contains(_file);
    }

    private void ToggleSelection()
    {
      if (_isSelected)
      {
        _selectedItems.Remove(_file);
      }
      else
      {
        _selectedItems.Add(_file);
      }
    }
  }
}