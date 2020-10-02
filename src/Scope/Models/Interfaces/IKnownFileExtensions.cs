using System;
using System.Collections.Generic;

namespace Scope.Models.Interfaces
{
  public interface IKnownFileExtensions
  {
    IEnumerable<string> All { get; }
    IEnumerable<string> Searchable { get; }
    event Action Changed;
  }
}
