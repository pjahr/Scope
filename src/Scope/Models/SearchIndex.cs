﻿using System;
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
    public event Action<bool> IsSearchingChanged;

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

      ResultsCleared.Raise();
      Began.Raise();

      var t = searchTerms.Aggregate((c, n) => $"{c}, {n}");
      Console.WriteLine($"Started search for {t}.");

      _task = Task.Run(() => FindItems(searchTerms, _currentP4K.FileSystem.TotalNumberOfFiles),
                       _cts.Token);
    }

    private readonly string[] _knownTextFileFormats =
    {
      ".txt", ".cfg", ".cfgf", ".cfgm", ".ini", ".id"
    };

    private void FindItems(string[] searchTerms, int numberOfFiles)
    {
      _results.Clear();

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
            if (f.Path.Contains(term.ToLowerInvariant()))
            {
              var match = new Match(term, f, MatchType.Path);
              Console.WriteLine($"Found '{term}' in '{f.Path}'");
              _uiDispatch.Do(() => MatchFound.Raise(match));
              _results.Add(match);
            }

            if (f.Name.Contains(term.ToLowerInvariant()))
            {
              var match = new Match(term, f, MatchType.Filename);
              Console.WriteLine($"Found '{term}' in '{f.Name}'");
              _uiDispatch.Do(() => MatchFound.Raise(match));
              _results.Add(match);
            }

            if (text.Contains(term.ToLowerInvariant()))
            {
              var match = new Match(term, f, MatchType.Content);
              Console.WriteLine($"Found '{term}' in content of {f.Name}");
              _uiDispatch.Do(() => MatchFound.Raise(match));
              _results.Add(match);
            }
          }
        }
      }

      Finished.Raise();
      var t = searchTerms.Aggregate((c, n) => $"{c}, {n}");
      Console.WriteLine($"Finished search for {t}.");
    }

    public void BuildUp() { }
  }
}
