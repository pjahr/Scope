using System.Collections.Generic;

namespace Scope.Models.Interfaces
{
  public interface ISearchOptions
  {
    SearchMode Mode { get; set; }
    bool SearchCaseSensitive { get; set; }
    ICollection<string> IncludeExtensions { get; }
  }
}
