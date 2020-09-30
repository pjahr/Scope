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
    private readonly IUiDispatch _uiDispatch;
    private readonly List<Match> _results = new List<Match>();
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

    public event Action ResultsCleared;
    public event Action<Match> MatchFound;
    public event Action Began;
    public event Action Finished;

    public IReadOnlyCollection<Match> Results => _results;

    public Search(ICurrentP4k currentP4K, IUiDispatch uiDispatch)
    {
      _currentP4K = currentP4K;
      _uiDispatch = uiDispatch;
    }

    public Task FindMatches(params string[] searchTerms)
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

      return Task.Factory.StartNew(() => FindItems(searchTerms, _currentP4K.FileSystem.TotalNumberOfFiles),
                                         _cts.Token);
    }

    private readonly string[] _knownTextFileFormats =
    {
      ".txt", ".cfg", ".cfgf", ".cfgm", ".ini", ".id", "json"
    };

    private void FindItems(string[] searchTerms, int numberOfFiles)
    {
      for (int i = 0; i < numberOfFiles; i++)
      {
        var f = _currentP4K.FileSystem[i];

        if (!_knownTextFileFormats.Any(ending => f.Name.EndsWith(ending)))
        {
          continue;
        }

        foreach (var term in searchTerms)
        {
          FindMatchInFileName(f, term);
          //FindMatchInContent(f, term);
        }
      }

      if (_results.Any())
      {
        Console.WriteLine(_results.Select(m => $"{m.File.Path}").Aggregate((c, n) => $"{c}\r\n{n}"));
      }

      Finished.Raise();
    }

    private void FindMatchInFileName(IFile f, string term)
    {
      if (_results.Any(match => match.File == f))
      {
        return; // provide only one match if the file name matches multiple terms
      }

      if (f.Name.Contains(term.ToLowerInvariant()))
      {
        var match = new Match(term, f, MatchType.Filename);

        _results.Add(match);
        _uiDispatch.Do(() => MatchFound.Raise(match));
      }
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
