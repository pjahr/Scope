using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace Scope.Models.Interfaces
{
  public interface ISearch
  {
    IEnumerable<Match> Results { get; }

    void InitiateSearchFor(params string[] searchTerms);

    event Action Began;

    event Action Finished;

    event Action<bool> IsSearchingChanged;

    event Action ResultsCleared;
    
    event Action<Match> MatchFound;

    void BuildUp();
  }
}
