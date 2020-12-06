using Scope.Interfaces;
using Scope.Models.Interfaces;

namespace Scope.ViewModels.Factories
{
  internal class DirectoryTreeNodeViewModelFactory : IDirectoryTreeNodeViewModelFactory
  {
    private readonly ISearch _search;
    private readonly ISearchOptions _searchOptions;
    private readonly IUiDispatch _uiDispatch;
    private readonly IFileTreeNodeViewModelFactory _fileTreeNodeViewModelFactory;

    public DirectoryTreeNodeViewModelFactory(ISearch search,
                                             ISearchOptions searchOptions,
                                             IUiDispatch uiDispatch,
                                             IFileTreeNodeViewModelFactory fileTreeNodeViewModelFactory)
    {
      _search = search;
      _searchOptions = searchOptions;
      _uiDispatch = uiDispatch;
      _fileTreeNodeViewModelFactory = fileTreeNodeViewModelFactory;
    }

    public DirectoryTreeNodeViewModel Create(IDirectory directory)
    {
      return new DirectoryTreeNodeViewModel(directory, 
                                            _search,
                                            _searchOptions,
                                            _uiDispatch,
                                            _fileTreeNodeViewModelFactory);
    }
  }
}
