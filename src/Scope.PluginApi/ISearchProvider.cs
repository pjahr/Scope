using System.Collections.Generic;

namespace Scope.Interfaces
{
  public interface ISearchProvider
  {
    IEnumerable<ISearchableFileType> FileTypes { get; }    
  }
}
