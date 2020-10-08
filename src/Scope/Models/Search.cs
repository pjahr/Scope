using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Scope.Interfaces;
using Scope.Models.Interfaces;
using Scope.Utils;

namespace Scope.Models
{
  [Export]
  internal class Search : ISearch
  {
    private readonly ICurrentP4k _currentP4K;
    private readonly ISearchOptions _searchOptions;
    private readonly IUiDispatch _uiDispatch;
    private readonly List<Match> _results = new List<Match>();
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

    public event Action ResultsCleared;
    public event Action<Match> MatchFound;
    public event Action Began;
    public event Action Finished;

    public IReadOnlyCollection<Match> Results => _results;

    public Search(ICurrentP4k currentP4K,
                  ISearchOptions searchOptions,
                  IUiDispatch uiDispatch)
    {
      _currentP4K = currentP4K;
      _searchOptions = searchOptions;
      _uiDispatch = uiDispatch;
    }

    public Task FindMatches(IProgress<SearchProgress> progress, params string[] searchTerms)
    {
      if (_currentP4K.FileSystem == null)
      {
        return Task.CompletedTask;
      }

      _results.Clear();
      ResultsCleared.Raise();

      // do not search for nothing or white space
      if (searchTerms.All(term => string.IsNullOrWhiteSpace(term)))
      {
        return Task.CompletedTask;
      }

      Began.Raise();

      return Task.Factory.StartNew(() => FindItems(searchTerms, _currentP4K.FileSystem.TotalNumberOfFiles, progress),
                                         _cts.Token);
    }    

    private void FindItems(string[] searchTerms, int numberOfFiles, IProgress<SearchProgress> progress)
    {
      for (int i = 0; i < numberOfFiles; i++)
      {
        var f = _currentP4K.FileSystem[i];

        if (!_searchOptions.IncludeExtensions.Any(ending => f.Name.EndsWith(ending)))
        {
          continue;
        }

        foreach (var term in searchTerms)
        {
          switch (_searchOptions.Mode)
          {
            case SearchMode.DirectoryName:
              FindMatchInFileName(f, term);
              break;
            case SearchMode.FileName:
              FindMatchInFileName(f, term);
              break;
            case SearchMode.FileContent:
              break;
            case SearchMode.FileNameAndContent:
              FindMatchInFileName(f, term);
              break;
            default:
              break;
          }
        }

        if ((i + 1) % 10 == 0)
        {
          progress.Report(new SearchProgress(10, numberOfFiles));
        }
      }

      progress.Report(new SearchProgress(0, 0)); // reset progress with magic null value
      Finished.Raise();
    }

    private void FindMatchInFileName(IFile f, string term)
    {
      if (_results.Any(match => match.File == f))
      {
        return; // provide only one match if the file name matches multiple terms
      }
      if (FileNameContains(f, term))
      {
        var match = new Match(term, f, MatchType.Filename);

        _results.Add(match);
        _uiDispatch.Do(() => MatchFound.Raise(match));
      }
    }

    private bool FileNameContains(IFile f, string term)
    {
      return _searchOptions.SearchCaseSensitive
        ? f.Name.Contains(term)
        : f.Name.ToLowerInvariant().Contains(term.ToLowerInvariant());
    }

    private void FindMatchInContent(IFile f, string term)
    {
      string text;
      using (var s = f.Read())
      {
        //TODO: this will prevent case-sensitive search
        text = Encoding.UTF8.GetString(s.ReadAllBytes())
                              .ToLowerInvariant();
      }

      if (text.Contains(term.ToLowerInvariant()))
      {
        var match = new Match(term, f, MatchType.Content);
        _uiDispatch.Do(() => MatchFound.Raise(match));
        _results.Add(match);
      }
    }
  }
}
