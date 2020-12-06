using Scope.Interfaces;
using Scope.Models.Interfaces;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace Scope.ViewModels.Factories
{
  [Export]
  internal class FileSystemTreeNodeViewModelFactory : IFileSystemTreeNodeViewModelFactory
  {
    private readonly ISearch _search;
    private readonly ISearchOptions _searchOptions;
    private readonly IUiDispatch _uiDispatch;
    private readonly IFileSubStructureProvider[] _allFileSubStructureProviders;

    public FileSystemTreeNodeViewModelFactory(ISearch search,
                                              ISearchOptions searchOptions,
                                              IUiDispatch uiDispatch,
                                              IFileSubStructureProvider[] allFileSubStructureProviders)
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
                                      matchingFileSubStructureProviders,
                                      this);
    }

    public DirectoryTreeNodeViewModel Create(IDirectory directory)
    {
      return new DirectoryTreeNodeViewModel(directory,
                                            _search,
                                            _searchOptions,
                                            _uiDispatch,
                                            this);
    }
  }
}
