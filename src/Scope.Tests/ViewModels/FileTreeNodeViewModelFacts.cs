using Moq;
using Scope.Interfaces;
using Scope.Models.Interfaces;
using Scope.ViewModels;
using Scope.ViewModels.Factories;
using System.Linq;
using Xunit;

namespace Scope.Tests.ViewModels
{
  public class FileTreeNodeViewModelFacts
  {
    private readonly ISearch _search;
    private readonly ISearchOptions _searchOptions;
    private readonly IUiDispatch _uiDispatch;
    private readonly IFile _file;
    private IFileSubStructureProvider[] _allFileSubStructureProviders;
    private FileTreeNodeViewModel _sut;

    public FileTreeNodeViewModelFacts()
    {
      _search = Mock.Of<ISearch>()
                .ReturnsOn(m=>m.FileResults, new FileMatch[0]);

      _searchOptions = Mock.Of<ISearchOptions>();

      _uiDispatch = Mock.Of<IUiDispatch>();      

      _file = Mock.Of<IFile>();
    }

    [Theory]
    [InlineData(new string[0], false)]
    [InlineData(new[] {".x" }, true)]
    [InlineData(new[] {".x", ".y" }, true)]
    [InlineData(new[] { ".y", ".x" }, true)]
    [InlineData(new[] { ".y" }, false)]
    [InlineData(new[] { ".y", ".z" }, false)]
    public void It_has_children_if_there_are_applicable_filesubstructureproviders_only(string[] providerExtensions, bool sutHasChildren)
    {
      const string fileName = "file.x";

      GivenThereAreProvidersFor(providerExtensions);
      GivenAFile(fileName);
      GivenSut();

      Assert.Equal(sutHasChildren, _sut.HasChildren);
    }

    private void GivenAFile(string fileName)
    {
      _file.ReturnsOn(m => m.Name, fileName);
    }

    private void GivenThereAreProvidersFor(string[] providerExtensions)
    {
      _allFileSubStructureProviders
        = providerExtensions
          .Select(text => Mock.Of<IFileSubStructureProvider>()
                          .ReturnsOn(m => m.ApplicableFileExtension, text))
          .ToArray();
    }

    private void GivenSut()
    {
      IFileSystemTreeNodeViewModelFactory factory
        = new FileSystemTreeNodeViewModelFactory(_search,
                                                 _searchOptions,
                                                 _uiDispatch,
                                                 _allFileSubStructureProviders);
      _sut = factory.Create(_file);
    }
  }
}
