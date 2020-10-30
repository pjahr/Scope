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
    private readonly List<string> _currentSearchTerms = new List<string>();
    private readonly Dictionary<IFile, FileMatch> _fileResults = new Dictionary<IFile, FileMatch>();
    private readonly List<string> _resultPaths = new List<string>();
    private readonly List<int> _resultIds = new List<int>();

    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

    public event Action ResultsCleared;
    public event Action Began;
    public event Action Finished;

    public IReadOnlyCollection<FileMatch> FileResults => _fileResults.Values;
    public IReadOnlyCollection<string> ResultPaths => _resultPaths;
    public IReadOnlyCollection<int> ResultIds => _resultIds;
    public IReadOnlyCollection<string> CurrentSearchTerms => _currentSearchTerms;

    public Search(ICurrentP4k currentP4K,
                  ISearchOptions searchOptions)
    {
      _currentP4K = currentP4K;
      _searchOptions = searchOptions;
    }

    public Task FindMatches(IProgress<SearchProgress> progress, params string[] searchTerms)
    {
      if (_currentP4K.FileSystem == null)
      {
        return Task.CompletedTask;
      }

      _currentSearchTerms.Clear();
      _currentSearchTerms.AddRange(searchTerms);

      _fileResults.Clear();
      _resultIds.Clear();
      _resultPaths.Clear();
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
      FindMatchesInFiles(searchTerms, numberOfFiles, progress);
      Finished.Raise();
    }

    private void FindMatchesInFiles(string[] searchTerms, int numberOfFiles, IProgress<SearchProgress> progress)
    {
      for (int i = 0; i < numberOfFiles; i++)
      {
        var f = _currentP4K.FileSystem[i];

        if (!_searchOptions.IncludeExtensions.Any(extension => f.Name.EndsWith(extension)))
        {
          continue;
        }

        foreach (var term in searchTerms)
        {
          FindMatchInFileName(f, term);
        }

        if ((i + 1) % 10 == 0)
        {
          progress.Report(new SearchProgress(10, numberOfFiles));
        }
      }

      progress.Report(new SearchProgress(0, 0)); // reset progress with magic null value
    }

    private void FindMatchInFileName(IFile f, string term)
    {
      if (_fileResults.ContainsKey(f))
      {
        return; // provide only one match if the file name matches multiple terms
      }
      if (FileNameContains(f, term))
      {
        var match = new FileMatch(term, f, MatchType.Filename);

        _fileResults.Add(f, match);
        _resultPaths.Add(f.Path);
        _resultIds.Add(f.Index);
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
        var match = new FileMatch(term, f, MatchType.Content);
        _fileResults.Add(f, match);
      }
    }
  }
}
