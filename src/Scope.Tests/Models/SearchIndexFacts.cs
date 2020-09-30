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
    private readonly IUiDispatch _uiDispatch = Mock.Of<IUiDispatch>();
    private readonly Action _resultsClearedWasRaised = Mock.Of<Action>();
    private readonly List<Scope.Models.Interfaces.Match> _results = new List<Scope.Models.Interfaces.Match>();

    private IFile[] _files;

    public SearchIndexFacts()
    {
      // ui dispatch should simply call the given actions (no UI thread handling in unit tests)
      _uiDispatch.Mock().Setup(m => m.Do(It.IsAny<Action>())).Callback((Action a) => a());

      _currentP4K.ReturnsOn(m => m.IsInitialized, true)
                       .ReturnsOn(m => m.FileSystem, _fileSystem);

      _fileSystem.Mock().Setup(m => m[It.IsAny<int>()]).Returns((int i) => _files[i]);
    }

    [Fact]
    public void It_clears_the_search_results_when_the_search_term_is_whitespace()
    {
      WhenSutIsCreated();      

      WhenSearchIsInitiatedWith("");

      ThenTheResultsAreEmpty();
      ThenTheEventWasRaised(1);
    }

    [Theory()]
    [InlineData("file.json", new[] { "f" })]
    [InlineData("file.json", new[] { "file" })]
    [InlineData("file.json", new[] { "e" })]
    [InlineData("file.json", new[] { "e", "s" })]
    public async void It_finds_an_item(string filename, string[] terms)
    {
      GivenFiles(AFile(filename));
      WhenSutIsCreated();

      await _sut.FindMatches(terms);

      ThenItfoundTheFile();
    }


    private void GivenFiles(params IFile[] files)
    {
      _files = files;
      _fileSystem.ReturnsOn(m => m.TotalNumberOfFiles, _files.Count());
    }

    private void WhenSutIsCreated()
    {
      _sut = new Search(_currentP4K, _uiDispatch);
      _sut.ResultsCleared += _resultsClearedWasRaised;
      _sut.MatchFound += _results.Add;
    }

    private void WhenSearchIsInitiatedWith(string terms)
    {
      _sut.FindMatches(terms);
    }

    private void ThenTheResultsAreEmpty()
    {
      Assert.Empty(_sut.Results);
    }

    private void ThenTheEventWasRaised(int expectedCalls)
    {
      _resultsClearedWasRaised.Mock().Verify(m => m(), Times.Exactly(expectedCalls));
    }

    private void ThenItfoundTheFile()
    {
      Assert.NotNull(_results.SingleOrDefault(r => r.File == _files[0]));
    }

    private static FileFake AFile(string filename)
    {
      return new FileFake()
      {
        Name = filename,
        Text = string.Empty
      };
    }
  }
}
