using System.Collections.Generic;

namespace Scope.Models
{
  internal class P4kFileStatistics
  {
    public P4kFileStatistics(Dictionary<string, int> fileTypes)
    {
      FileTypes = fileTypes;
    }

    public IReadOnlyDictionary<string, int> FileTypes { get; private set; }
  }
}
