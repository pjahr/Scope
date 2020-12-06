using Scope.Interfaces;
using Scope.Models.Interfaces;
using System.IO;
using System.Linq;

namespace Scope.ViewModels.Factories
{
  internal class FileTreeNodeViewModelFactory : IFileTreeNodeViewModelFactory
  {
    private readonly ISearch _search;
    private readonly ISearchOptions _searchOptions;
    private readonly IUiDispatch _uiDispatch;
    private readonly IFileSubStructureProvider[] _allFileSubStructureProviders;

    public FileTreeNodeViewModelFactory(ISearch search,
                                        ISearchOptions searchOptions,
                                        IUiDispatch uiDispatch,
                                        IFileSubStructureProvider[] allFileSubStructureProviders, idi)
    {
      _search = search;
      _searchOptions = searchOptions;
      _uiDispatch = uiDispatch;
      _allFileSubStructureProviders = allFileSubStructureProviders;
    }

    public FileTreeNodeViewModel Create(IFile file)
    {
      var matchingFileSubStructureProviders
          = _allFileSubStructureProviders
            .Where(p => p.ApplicableFileExtension
                     == Path.GetExtension(file.Name))
            .ToArray();

      return new FileTreeNodeViewModel(file,
                                      _search,
                                      _searchOptions,
                                      _uiDispatch,
                                      matchingFileSubStructureProviders);
    }
  }
}
