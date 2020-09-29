using Moq;
using Scope.Interfaces;
using Scope.Models;
using Scope.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Scope.Tests.Models
{
  public class SearchIndexFacts
  {
    private ISearch _sut;
    
    private readonly ICurrentP4k _currentP4K = Mock.Of<ICurrentP4k>();
    private readonly IFileSystem _fileSystem = Mock.Of<IFileSystem>();
    private readonly IUiDispatch _uiDispatch;
    private readonly Action _resultsClearedWasRaised = Mock.Of<Action>();
    private readonly List<Scope.Models.Interfaces.Match> _results = new List<Scope.Models.Interfaces.Match>();

    private IFile[] _files;

    [Fact]
    public void It_clears_the_search_results_when_the_search_term_is_whitespace()
    {
      GivenACurrentPak();

      WhenSutIsCreated();

      _sut.ResultsCleared += _resultsClearedWasRaised;

      WhenSearchIsInitiatedWith("");

      ThenTheResultsAreEmpty();
      ThenTheEventWasRaised(1);
    }

    [Theory()]
    [InlineData("file.json", new[] { "f" })]
    public void It_finds_(string filename, string[] terms)
    {
      _files = new[]
      {
        new FileFake()
        {
          Name=filename,
          Text=string.Empty
        }
      };

      _currentP4K.ReturnsOn(m => m.IsInitialized, true)
                 .ReturnsOn(m => m.FileSystem, _fileSystem);

      _fileSystem.ReturnsOn(m => m.TotalNumberOfFiles, 1);
      _fileSystem.Mock().Setup(m => m[It.IsAny<int>()]).Returns((int i) => _files[i]);

      WhenSutIsCreated();

      _sut.InitiateSearchFor(terms);

      Assert.NotNull(_results.SingleOrDefault(r => r.File == _files[0]));
    }

    private void GivenACurrentPak()
    {
      _currentP4K.ReturnsOn(m => m.IsInitialized, true)
                       .ReturnsOn(m => m.FileSystem, _fileSystem);
    }

    private void WhenSutIsCreated()
    {
      _sut = new SearchIndex(_currentP4K, _uiDispatch);
      _sut.ResultsCleared += _resultsClearedWasRaised;
      _sut.MatchFound += _results.Add;
    }

    private void WhenSearchIsInitiatedWith(string terms)
    {
      _sut.InitiateSearchFor(terms);
    }

    private void ThenTheResultsAreEmpty()
    {
      Assert.Empty(_sut.Results);
    }

    private void ThenTheEventWasRaised(int expectedCalls)
    {
      _resultsClearedWasRaised.Mock().Verify(m => m(), Times.Exactly(expectedCalls));
    }


  }
}
