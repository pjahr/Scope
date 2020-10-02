using Scope.Models.Interfaces;
using Scope.Utils;
using Scope.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;

namespace Scope.ViewModels
{
  [Export]
  public class SearchOptionsViewModel : INotifyPropertyChanged
  {
    private readonly ISearchOptions _searchOptions;

    public SearchOptionsViewModel(ISearchOptions searchOptions,
                                  IKnownFileExtensions knownFileTypes)
    {
      _searchOptions = searchOptions;

      IncludedExtensions = knownFileTypes.All
                                         .OrderBy(x => x)
                                         .Select(x => CreateIncludedExtension(x))
                                         .ToList();
      SelectAllFileExtensionsCommand = new RelayCommand(SelectAllFileExtensions);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public IReadOnlyCollection<IncludedExtensionViewModel> IncludedExtensions { get; private set; }
    public ICommand SelectAllFileExtensionsCommand { get; }
    
    public SearchMode SearchMode
    {
      get => _searchOptions.Mode;
      set
      {
        if (_searchOptions.Mode == value)
        {
          return;
        }
        _searchOptions.Mode = value;
        PropertyChanged.Raise(this, nameof(SearchMode));
      }
    }

    public bool SearchCaseSensitive
    {
      get => _searchOptions.SearchCaseSensitive;
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

    private IncludedExtensionViewModel CreateIncludedExtension(string extension)
    {
      return new IncludedExtensionViewModel(extension);
    }

    private void SelectAllFileExtensions()
    {
      foreach (var item in IncludedExtensions)
      {
        item.IsIncluded = true;
      }
    }

  }
}