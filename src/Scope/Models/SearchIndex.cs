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
  internal class SearchIndex : ISearch
  {
    private readonly ICurrentP4k _currentP4K;
    private readonly IUiDispatch _uiDispatch;
    private readonly List<Match> _results = new List<Match>();

    public event Action ResultsCleared;
    public event Action<Match> MatchFound;
    public event Action Began;
    public event Action Finished;
    
    public IReadOnlyCollection<Match> Results => _results;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private Task _task;

    public SearchIndex(ICurrentP4k currentP4K, IUiDispatch uiDispatch)
    {
      _currentP4K = currentP4K;
      _uiDispatch = uiDispatch;
    }

    public void InitiateSearchFor(params string[] searchTerms)
    {
      if (_currentP4K.FileSystem == null)
      {
        return;
      }

      _results.Clear();
      ResultsCleared.Raise();

      Began.Raise();

      // do not search for nothing or white space
      if (searchTerms.All(term=>string.IsNullOrWhiteSpace(term)))
      {
        Finished.Raise();
        return;
      }

      _task = Task.Run(() => FindItems(searchTerms, _currentP4K.FileSystem.TotalNumberOfFiles),
                       _cts.Token);
    }

    private readonly string[] _knownTextFileFormats =
    {
      ".txt", ".cfg", ".cfgf", ".cfgm", ".ini", ".id"
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

        using (var s = f.Read())
        {
          string text = Encoding.UTF8.GetString(s.ReadAllBytes())
                                .ToLowerInvariant();
          foreach (var term in searchTerms)
          {
            if (f.Name.Contains(term.ToLowerInvariant()))
            {
              var match = new Match(term, f, MatchType.Filename);
              _uiDispatch.Do(() => MatchFound.Raise(match));
              _results.Add(match);
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

      Finished.Raise();
    }

    public void BuildUp() { }
  }
}
