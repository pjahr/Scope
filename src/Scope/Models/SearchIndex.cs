using Scope.Interfaces;
using Scope.Models.Interfaces;
using Scope.Utils;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Scope.Models
{
  [Export]
  internal class SearchIndex : ISearch
  {
    private readonly ICurrentP4k _currentP4K;
    private readonly IUiDispatch _uiDispatch;

    public event System.Action ResultsCleared;
    public event System.Action<Match> MatchFound;
    public event System.Action Began;
    public event System.Action Finished;
    public event System.Action<bool> IsSearchingChanged;

    public IEnumerable<Match> Results { get; }

    private CancellationTokenSource _cts = new CancellationTokenSource();
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

      var c = _currentP4K.FileSystem.TotalNumberOfFiles;

      ResultsCleared.Raise();

      _task = Task.Run(() => FindItems(searchTerms, c), _cts.Token);
    }

    private void FindItems(string[] searchTerms, int c)
    {
      for (int i = 0; i < c; i++)
      {
        var f = _currentP4K.FileSystem[i];

        if (!f.Name.EndsWith(".json"))
        {
          continue;
        }

        using (var s = f.Read())
        {
          string text = Encoding.UTF8.GetString(s.ReadAllBytes()).ToLowerInvariant();
          foreach (var term in searchTerms)
          {
            if (f.Path.Contains(term.ToLowerInvariant()))
            {
              var match = new Match(term, f, MatchType.Path);
              System.Console.WriteLine($"Found '{term}' in '{f.Path}'");
              _uiDispatch.Do(()=>MatchFound.Raise(match));
            }

            if (f.Name.Contains(term.ToLowerInvariant()))
            {
              var match = new Match(term, f, MatchType.Filename);
              System.Console.WriteLine($"Found '{term}' in '{f.Name}'");
              _uiDispatch.Do(() => MatchFound.Raise(match));
            }

            if (text.Contains(term.ToLowerInvariant()))
            {
              var match = new Match(term, f, MatchType.Content);
              System.Console.WriteLine($"Found '{term}' in content of {f.Name}");
              _uiDispatch.Do(() => MatchFound.Raise(match));
            }
          }
        }

      }
    }

    public void BuildUp()
    {
    }
  }
}
