﻿using Scope.Models.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Scope.Models
{
  [Export]
  internal class SearchOptions : ISearchOptions
  {
    public bool FindDirectories { get; set; }
    public bool SearchCaseSensitive { get; set; }
    public bool SearchContents { get; set; }
    public ICollection<string> IncludeExtensions { get; }
  }
}
