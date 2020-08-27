using Moq;
using Scope.Interfaces;
using Scope.Models;
using Scope.Models.Interfaces;
using System;
using Xunit;

namespace Scope.Tests.Models
{
  public class SearchIndexFacts
  {
    private ISearch _sut;
    private ICurrentP4k _currentP4K=Mock.Of<ICurrentP4k>();
    private IFileSystem _fileSystem =Mock.Of<IFileSystem>();
    private Action _eventWasRaised = Mock.Of<Action>();
    private IFile[] _files;
    private IUiDispatch _uiDispatch;

    [Fact]
    public void It_clears_the_search_results_when_a_term_is_searched()
    {
      _files = new[]
      {
        new FileFake()
        {
          Name="1 match.json",
          Text="one"
        },

        new FileFake()
        {
          Name="nothing.json",
          Text=""
        },

        new FileFake()
        {
          Name="noo match.json",
          Text="this does not match"
        },

        new FileFake()
        {
          Name="two matches",
          Text="one"
        },

        new FileFake()
        {
          Name="casing",
          Text="TWO ONE"
        }
      };

      _currentP4K.ReturnsOn(m => m.IsInitialized, true)
                 .ReturnsOn(m => m.FileSystem, _fileSystem);

      _fileSystem.ReturnsOn(m => m.TotalNumberOfFiles, 10);
      _fileSystem.Mock().Setup(m => m[It.IsAny<int>()]).Returns((int i) => _files[i]);

      _sut = new SearchIndex(_currentP4K, _uiDispatch);

      _sut.ResultsCleared += _eventWasRaised;

      _sut.InitiateSearchFor("one");

      Assert.Empty(_sut.Results);
      _eventWasRaised.Mock().Verify(m => m(), Times.Exactly(1));
    }
  }
}
