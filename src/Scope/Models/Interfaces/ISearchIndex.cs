using System.Collections.Generic;
using System.Threading.Tasks;

namespace Scope.Models.Interfaces
{
  public interface ISearchIndex
  {
    Task<int> AllOff(IReadOnlyCollection<string> searchTerms);
    Task<int> Either(IReadOnlyCollection<string> searchTerms);
    void BuildUp();
  }
}
