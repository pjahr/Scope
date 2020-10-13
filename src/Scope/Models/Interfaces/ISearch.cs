﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Scope.Models.Interfaces
{
  public interface ISearch
  {
    IReadOnlyCollection<Match> Results { get; }
    IReadOnlyCollection<string> ResultPaths { get; }
    IReadOnlyCollection<int> ResultIds { get; }

    Task FindMatches(IProgress<SearchProgress> progress, params string[] searchTerms);

    event Action Began;

    event Action Finished;

    event Action ResultsCleared;

    event Action<Match> MatchFound;
  }
}
