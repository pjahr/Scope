using System.Collections.Generic;

namespace Scope.Models.Interfaces
{
  public interface ISearchOptions
  {
    bool FindDirectories { get; set; }
    bool SearchCaseSensitive { get; set; }
    bool SearchContents { get; set; }
    ICollection<string> IncludeExtensions { get; }
  }
}
