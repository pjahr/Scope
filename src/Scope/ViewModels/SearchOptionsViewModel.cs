using Scope.Models.Interfaces;
using Scope.Utils;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

namespace Scope.ViewModels
{
  [Export]
  public class SearchOptionsViewModel : INotifyPropertyChanged
  {
    private readonly ISearchOptions _searchOptions;
    private readonly IKnownFileExtensions _knownFileTypes;
    private bool _searchAllSearchableFileTypes;
    private bool _searchAllFileTypes;

    public SearchOptionsViewModel(ISearchOptions searchOptions,
                                  IKnownFileExtensions knownFileTypes)
    {
      _searchOptions = searchOptions;
      _knownFileTypes = knownFileTypes;

      IncludedExtensions = new ObservableCollection<IncludedExtensionViewModel>();
      UpdateKnownFileTypes();      

      _knownFileTypes.Changed += UpdateKnownFileTypes;
    }

    private void UpdateKnownFileTypes()
    {
      IncludedExtensions.Clear();

      foreach (var extension in _knownFileTypes.All
                                               .OrderBy(x => x)
                                               .Select(x => CreateIncludedExtension(x))
                                               .ToList())
      {
        _searchOptions.IncludeExtensions.Add(extension.Name);
        IncludedExtensions.Add(extension);
        SearchAllFileTypes = true;
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<IncludedExtensionViewModel> IncludedExtensions { get; private set; }
    
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
        if (_searchOptions.Mode!=SearchMode.DirectoryName)
        {
          return;
        }
      }
    }

    public bool SearchCaseSensitive
    {
      get => _searchOptions.SearchCaseSensitive;
      set
      {
        if (_searchOptions.SearchCaseSensitive == value)
        {
          return;
        }
        _searchOptions.SearchCaseSensitive = value;
        PropertyChanged.Raise(this, nameof(SearchCaseSensitive));
      }
    }

    public bool SearchAllFileTypes
    {
      get => _searchAllFileTypes;
      set
      {
        if (_searchAllFileTypes == value)
        {
          return;
        }
        _searchAllFileTypes = value;
        PropertyChanged.Raise(this, nameof(SearchAllFileTypes));

        if (_searchAllFileTypes)
        {
          foreach (var item in IncludedExtensions)
          {
            item.IsIncluded = true;
          }
        }
        else
        {
          foreach (var item in IncludedExtensions)
          {
            item.IsIncluded = false;
          }
        }
      }
    }

    public bool SearchAllSearchableFileTypes
    {
      get => _searchAllSearchableFileTypes;
      set
      {
        if (_searchAllSearchableFileTypes == value)
        {
          return;
        }
        _searchAllSearchableFileTypes = value;        

        PropertyChanged.Raise(this, nameof(SearchAllSearchableFileTypes));

        if (_searchAllSearchableFileTypes)
        {
          // if the user ticks 'searchable' all known searchables get ticked, all others unticked
          foreach (var item in IncludedExtensions)
          {
            item.IsIncluded = _knownFileTypes.Searchable.Contains(item.Name);
          }
        }
        else
        {
          // if the user unticks 'searchable' all file types get unticked
          foreach (var item in IncludedExtensions)
          {
            item.IsIncluded = false;
          }
        }
      }
    }

    private IncludedExtensionViewModel CreateIncludedExtension(string extension)
    {
      var vm = new IncludedExtensionViewModel(extension);

      vm.IsIncludedChanged += UpdateInModel;

      return vm;
    }

    private void UpdateInModel(string extension, bool isIncluded)
    {
      if (isIncluded)
      {
        if (!_searchOptions.IncludeExtensions.Contains(extension))
        {
          _searchOptions.IncludeExtensions.Add(extension);
        }
      }
      else
      {
        if(_searchOptions.IncludeExtensions.Contains(extension))
        {
          _searchOptions.IncludeExtensions.Remove(extension);
        }
      }
    }
  }
}