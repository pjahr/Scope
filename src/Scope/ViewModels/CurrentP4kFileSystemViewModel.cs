using Scope.Interfaces;
using Scope.Models.Interfaces;
using Scope.Utils;
using Scope.ViewModels.Factories;
using System.ComponentModel;
using System.ComponentModel.Composition;

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
    private readonly IUiDispatch _uiDispatch;
    private readonly IFileSystemTreeNodeViewModelFactory _fileSystemTreeNodeViewModelFactory;

    public CurrentP4kFileSystemViewModel(ICurrentP4k currentP4k,
                                         ICurrentItem currentFile,
                                         IPinnedItems selectedItems,
                                         IExtractP4kContent extractP4KContent,
                                         ISearch search,
                                         IUiDispatch uiDispatch,
                                         SearchOptionsViewModel searchOptionsViewModel,
                                         IFileSystemTreeNodeViewModelFactory fileSystemTreeNodeViewModelFactory)
    {
      _currentP4K = currentP4k;
      _currentFile = currentFile;
      _selectedItems = selectedItems;
      _extractP4KContent = extractP4KContent;
      _search = search;
      _uiDispatch = uiDispatch;
      SearchOptionsViewModel = searchOptionsViewModel;
      _fileSystemTreeNodeViewModelFactory = fileSystemTreeNodeViewModelFactory;
      Initialize();

      _currentP4K.Changed += Initialize;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    
    public P4kFileSystemViewModel FileSystem { get; set; }
    public SearchOptionsViewModel SearchOptionsViewModel { get; }

    private void Initialize()
    {
      FileSystem = _currentP4K.FileSystem != null
                     ? new P4kFileSystemViewModel(_currentP4K.FileSystem,
                                                  _currentFile,
                                                  _selectedItems,
                                                  _extractP4KContent,
                                                  _fileSystemTreeNodeViewModelFactory,
                                                  _search,
                                                  _uiDispatch)
                     : null;

      PropertyChanged.Raise(this, nameof(FileSystem));
    }

  }
}