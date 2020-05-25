using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Scope.Models.Interfaces;
using Scope.ViewModels.Commands;

namespace Scope.ViewModels
{
  internal class P4kFileSystemViewModel
  {
    private readonly IFileSystem _fileSystem;
    private readonly ICurrentItem _currentItem;
    private readonly IPinnedItems _selectedItems;

    public P4kFileSystemViewModel(IFileSystem fileSystem,
                                  ICurrentItem currentItem,
                                  IPinnedItems selectedItems)
    {
      _fileSystem = fileSystem;
      _currentItem = currentItem;
      _selectedItems = selectedItems;

      RootItems = new ObservableCollection<object>();

      SetCurrentItemCommand = new RelayCommand<object>(SetCurrentItem);
      SetCurrentFileToNothingCommand = new RelayCommand(_currentItem.Clear);
      ToggleSelectionOfCurrentItemCommand = new RelayCommand(ToggleSelectionOfCurrentItem);

      ExpandCommand = new RelayCommand<object>(async p =>
      {
        if (!(p is DirectoryViewModel directory))
        {
          return;
        }

        await directory.LoadChildrenAsync();
      });

      ExtractCommand = new RelayCommand<object>(p => 
      {
        
      });

      CreateContainedDirectories();
      CreateContainedFiles();
    }

    private void SetCurrentItem(object item)
    {
      if (item is FileViewModel file)
      {
        _currentItem.ChangeTo(file.File);
        return;
      }

      if (item is DirectoryViewModel directory)
      {
        _currentItem.ChangeTo(directory.Directory);
      }
    }

    private void ToggleSelectionOfCurrentItem()
    {
      if (_currentItem.CurrentDirectory != null)
      {
        if (_selectedItems.Directories.Contains(_currentItem.CurrentDirectory))
        {
          _selectedItems.Remove(_currentItem.CurrentDirectory);
        }
        else
        {
          _selectedItems.Add(_currentItem.CurrentDirectory);
        }

        return;
      }

      if (_currentItem.CurrentFile != null)
      {
        if (_selectedItems.Files.Contains(_currentItem.CurrentFile))
        {
          _selectedItems.Remove(_currentItem.CurrentFile);
        }
        else
        {
          _selectedItems.Add(_currentItem.CurrentFile);
        }
      }
    }

    public ObservableCollection<object> RootItems { get; private set; }
    public ICommand SetCurrentItemCommand { get; }
    public ICommand SetCurrentFileToNothingCommand { get; }
    public ICommand ToggleSelectionOfCurrentItemCommand { get; }

    public ICommand ExpandCommand { get; }
    public ICommand ExtractCommand { get; }

    private void CreateContainedFiles()
    {
      foreach (var vm in _fileSystem.Root.Files.Select(d => new FileViewModel(d, null)))
      {
        RootItems.Add(vm);
      }
    }

    private void CreateContainedDirectories()
    {
      foreach (var vm in _fileSystem.Root.Directories.Select(d => new DirectoryViewModel(d, null)))
      {
        RootItems.Add(vm);
      }
    }
  }
}
