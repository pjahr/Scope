using Scope.Interfaces;
using Scope.Models.Interfaces;
using Scope.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

namespace Scope.ViewModels
{
  [Export]
  internal class CurrentP4kFileSystemViewModel : INotifyPropertyChanged
  {
    private readonly ICurrentP4k _currentP4K;
    private readonly ICurrentItem _currentFile;
    private readonly IPinnedItems _selectedItems;
    private readonly IExtractP4kContent _extractP4KContent;
    private readonly ISearch _search;
    private readonly ISearchOptions _searchOptions;
    private readonly IUiDispatch _uiDispatch;
    private readonly IFileSubStructureProvider[] _fileSubStructureProviders;

    public CurrentP4kFileSystemViewModel(ICurrentP4k currentP4k,
                                         ICurrentItem currentFile,
                                         IPinnedItems selectedItems,
                                         IExtractP4kContent extractP4KContent,
                                         ISearch search,
                                         ISearchOptions searchOptions,
                                         IUiDispatch uiDispatch,
                                         SearchOptionsViewModel searchOptionsViewModel,
                                         IEnumerable<IFileSubStructureProvider> fileSubStructureProviders = null)
    {
      _currentP4K = currentP4k;
      _currentFile = currentFile;
      _selectedItems = selectedItems;
      _extractP4KContent = extractP4KContent;
      _search = search;
      _searchOptions = searchOptions;
      _uiDispatch = uiDispatch;
      _fileSubStructureProviders = fileSubStructureProviders != null ? fileSubStructureProviders.ToArray() : new IFileSubStructureProvider[0];
      SearchOptionsViewModel = searchOptionsViewModel;

      Initialize();

      _currentP4K.Changed += Initialize;
    }

    private void Initialize()
    {
      FileSystem = _currentP4K.FileSystem != null
                     ? new P4kFileSystemViewModel(_currentP4K.FileSystem,
                                                  _currentFile,
                                                  _selectedItems,
                                                  _extractP4KContent,
                                                  _search,
                                                  _searchOptions,
                                                  _uiDispatch,
                                                  _fileSubStructureProviders)
                     : null;

      PropertyChanged.Raise(this, nameof(FileSystem));
    }

    public P4kFileSystemViewModel FileSystem { get; set; }
    public SearchOptionsViewModel SearchOptionsViewModel { get; }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}