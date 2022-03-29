using Scope.Models.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Scope.Models
{
  [Export]
  internal class SearchOptions : ISearchOptions
  {
    public SearchOptions()
    {
      Mode = SearchMode.FileName;
      SearchCaseSensitive = false;
      IncludeExtensions = new List<string>();
    }

    public SearchMode Mode { get; set; }
    public bool SearchCaseSensitive { get; set; }
    public ICollection<string> IncludeExtensions { get; }
  }
}
