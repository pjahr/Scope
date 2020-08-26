using System;
using System.Collections.Generic;

namespace Scope.Models.Interfaces
{
  public interface ISearchIndex
  {
    IEnumerable<Match> Results { get; }

    void Either(params string[] searchTerms);

    event Action ResultsCleared;
    event Action<Match> MatchFound;

    void BuildUp();
  }
}
