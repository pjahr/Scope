using System;
using System.Collections.Generic;

namespace Scope.Models.Interfaces
{
  public interface ISearch
  {
    IReadOnlyCollection<Match> Results { get; }

    void InitiateSearchFor(params string[] searchTerms);

    event Action Began;

    event Action Finished;

    event Action<bool> IsSearchingChanged;

    event Action ResultsCleared;

    event Action<Match> MatchFound;

    void BuildUp();
  }
}
