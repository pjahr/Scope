using Scope.Models.Interfaces;
using Scope.Utils;
using Scope.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Security.RightsManagement;
using System.Windows.Input;

namespace Scope.ViewModels
{
  [Export]
  public class SearchOptionsViewModel : INotifyPropertyChanged
  {
    private readonly ISearchOptions _searchOptions;

    public SearchOptionsViewModel(ISearchOptions searchOptions, IKnownFileExtensions knownFileTypes)
    {
      _searchOptions = searchOptions;

      IncludedExtensions =knownFileTypes.All
                                         .OrderBy(x => x)
                                         .Select(x => new IncludedExtensionViewModel(x))
                                         .ToList();
      SelectAllFileExtensionsCommand = new RelayCommand(SelectAllFileExtensions);
    }

    private void SelectAllFileExtensions()
    {
      foreach (var item in IncludedExtensions)
      {

      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public IReadOnlyCollection<IncludedExtensionViewModel> IncludedExtensions { get; private set; }
    public ICommand SelectAllFileExtensionsCommand { get; }
    
    public bool FindDirectories
    {
      get => _searchOptions.FindDirectories;
      private set
      {
        if (_searchOptions.FindDirectories == value)
        {
          return;
        }
        _searchOptions.FindDirectories = value;
        PropertyChanged.Raise(this, nameof(FindDirectories));
      }
    }

    public bool SearchCaseSensitive
    {
      get => _searchOptions.FindDirectories;
      private set
      {
        if (_searchOptions.SearchCaseSensitive == value)
        {
          return;
        }
        _searchOptions.SearchCaseSensitive = value;
        PropertyChanged.Raise(this, nameof(SearchCaseSensitive));
      }
    }

    public bool SearchContents
    {
      get => _searchOptions.SearchContents;
      private set
      {
        if (_searchOptions.SearchContents == value)
        {
          return;
        }
        _searchOptions.SearchContents = value;
        PropertyChanged.Raise(this, nameof(SearchContents));
      }
    }

  }
}