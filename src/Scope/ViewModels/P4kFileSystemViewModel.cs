using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Scope.Interfaces;
using Scope.Models.Interfaces;
using Scope.Utils;
using Scope.ViewModels.Factories;

namespace Scope.ViewModels
{
  internal class P4kFileSystemViewModel : INotifyPropertyChanged
  {
    private readonly IFileSystem _fileSystem;
    private readonly ICurrentItem _currentItem;
    private readonly IPinnedItems _selectedItems;
    private readonly IExtractP4kContent _extractP4KContent;
    private readonly IFileSystemTreeNodeViewModelFactory _fileSystemTreeNodeViewModelFactory;
    private readonly ISearch _search;
    private readonly IUiDispatch _uiDispatch;
    
    public P4kFileSystemViewModel(IFileSystem fileSystem,
                                  ICurrentItem currentItem,
                                  IPinnedItems selectedItems,
                                  IExtractP4kContent extractP4KContent,
                                  IFileSystemTreeNodeViewModelFactory fileSystemTreeNodeViewModelFactory,
                                  ISearch search,
                                  IUiDispatch uiDispatch)
    {
      _fileSystem = fileSystem;
      _currentItem = currentItem;
      _selectedItems = selectedItems;
      _extractP4KContent = extractP4KContent;
      _fileSystemTreeNodeViewModelFactory = fileSystemTreeNodeViewModelFactory;
      _search = search;
      _uiDispatch = uiDispatch;     

      RootItems = new ObservableCollection<TreeNodeViewModel>();
      SetCurrentItemCommand = new RelayCommand<object>(SetCurrentItem);
      SetCurrentFileToNothingCommand = new RelayCommand(_currentItem.Clear);
      ToggleSelectionOfCurrentItemCommand = new RelayCommand(ToggleSelectionOfCurrentItem);
      ExtractCommand = new RelayCommand<object>(ExtractItem);

      CreateRootItems();

      _search.Finished += FilterRootItems;
      _search.ResultsCleared += CreateRootItems;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public SearchFoundNoMatchViewModel SearchFoundNoMatch
    {
      get;
      private set;
    }

    private void CreateRootItems()
    {
      foreach (var disposable in RootItems)
      {
        disposable.Dispose();
      }

      _uiDispatch.Do(() =>
      {
        RootItems.Clear();

        CreateContainedDirectories();
        CreateContainedFiles();
      });

      SearchFoundNoMatch = null;
      PropertyChanged.Raise(this, nameof(SearchFoundNoMatch));
    }

    private void FilterRootItems()
    {
      var hidden = RootItems.Where(i => !_search.FileResults.Any(r => r.File.Path.StartsWith(i.Path)))
                                  .ToArray();

      foreach (var item in hidden)
      {
        item.Dispose();
        _uiDispatch.Do(() => RootItems.Remove(item));
      }

      // show info if no results
      SearchFoundNoMatch = _search.FileResults.Any() ? null : new SearchFoundNoMatchViewModel();
      PropertyChanged.Raise(this, nameof(SearchFoundNoMatch));
    }

    public ObservableCollection<TreeNodeViewModel> RootItems { get; private set; }

    public ICommand SetCurrentItemCommand { get; }
    public ICommand SetCurrentFileToNothingCommand { get; }
    public ICommand ToggleSelectionOfCurrentItemCommand { get; }
    public ICommand ExtractCommand { get; }

    private void SetCurrentItem(object item)
    {
      if (item is FileTreeNodeViewModel file)
      {
        _currentItem.ChangeTo(file.Model);
        return;
      }

      if (item is DirectoryTreeNodeViewModel directory)
      {
        _currentItem.ChangeTo(directory.Model);
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

    private void CreateContainedFiles()
    {
      foreach (var file in _fileSystem.Root.Files.OrderBy(f=>f.Name))
      {
        RootItems.Add(_fileSystemTreeNodeViewModelFactory.Create(file));
      }
    }

    private void CreateContainedDirectories()
    {
      foreach (var directory in _fileSystem.Root.Directories.OrderBy(d => d.Name))
      {
        RootItems.Add(_fileSystemTreeNodeViewModelFactory.Create(directory));
      }
    }

    private void ExtractItem(object item)
    {
      switch (item)
      {
        case DirectoryTreeNodeViewModel directory:
          ExtractDirectory(directory);
          return;
        case FileTreeNodeViewModel file:
          ExtractFile(file);
          return;
      }
    }

    private void ExtractFile(FileTreeNodeViewModel file)
    {
      _extractP4KContent.Extract(file.Model);
    }

    private void ExtractDirectory(DirectoryTreeNodeViewModel directory)
    {
      _extractP4KContent.Extract(directory.Model);
    }
  }
}
