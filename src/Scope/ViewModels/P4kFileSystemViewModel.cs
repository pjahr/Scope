﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Scope.Interfaces;
using Scope.Models.Interfaces;
using Scope.Utils;
using Scope.ViewModels.Commands;

namespace Scope.ViewModels
{
  internal class P4kFileSystemViewModel : INotifyPropertyChanged
  {
    private readonly IFileSystem _fileSystem;
    private readonly ICurrentItem _currentItem;
    private readonly IPinnedItems _selectedItems;
    private readonly IExtractP4kContent _extractP4KContent;
    private readonly ISearch _search;
    private readonly ISearchOptions _searchOptions;
    private readonly IUiDispatch _uiDispatch;
    private readonly IFileSubStructureProvider[] _subFileFactories;

    public P4kFileSystemViewModel(IFileSystem fileSystem,
                                  ICurrentItem currentItem,
                                  IPinnedItems selectedItems,
                                  IExtractP4kContent extractP4KContent,
                                  ISearch search,
                                  ISearchOptions searchOptions,
                                  IUiDispatch uiDispatch,
                                  IEnumerable<IFileSubStructureProvider> subFileFactories)
    {
      _fileSystem = fileSystem;
      _currentItem = currentItem;
      _selectedItems = selectedItems;
      _extractP4KContent = extractP4KContent;
      _search = search;
      _searchOptions = searchOptions;
      _uiDispatch = uiDispatch;
      _subFileFactories = subFileFactories.ToArray();
      RootItems = new ObservableCollection<TreeNodeViewModel>();

      SetCurrentItemCommand = new RelayCommand<object>(SetCurrentItem);
      SetCurrentFileToNothingCommand = new RelayCommand(_currentItem.Clear);
      ToggleSelectionOfCurrentItemCommand = new RelayCommand(ToggleSelectionOfCurrentItem);
      ExtractCommand = new RelayCommand<object>(ExtractItem);

      ExpandCommand = new RelayCommand<object>(async p =>
      {
        if (!(p is DirectoryTreeNodeViewModel directory))
        {
          return;
        }

        await directory.LoadChildrenAsync();
      });

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

    public ICommand ExpandCommand { get; }
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
      foreach (var vm in _fileSystem.Root.Files.Select(d => new FileTreeNodeViewModel(d, _search, _subFileFactories)))
      {
        RootItems.Add(vm);
      }
    }

    private void CreateContainedDirectories()
    {
      var rootDirectories = _fileSystem.Root.Directories.Select(d => new DirectoryTreeNodeViewModel(d, _search, _searchOptions, _uiDispatch, _subFileFactories));
      foreach (var directory in rootDirectories)
      {
        RootItems.Add(directory);
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
