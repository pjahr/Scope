using System.Collections.Generic;

namespace Scope.Utils
{
  public static class EnumerableExtensions
  {
    /// <summary>
    /// Returns an empty array of <see cref="T"/> if the list is null.
    /// </summary>
    public static IEnumerable<T> EmptyWhenNull<T>(this IEnumerable<T> subject)
    {
      return subject ?? new T[0];
    }
  }
}
