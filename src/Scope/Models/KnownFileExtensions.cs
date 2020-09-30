using System.Collections.Generic;
using System.ComponentModel.Composition;
using Scope.Models.Interfaces;

namespace Scope.Models
{
  [Export]
  internal class KnownFileExtensions : IKnownFileExtensions
  {
    public IEnumerable<string> All { get; } = new[] { ".txt", ".cfg", ".cfgf", ".cfgm", ".ini", ".id", "json" };
  }
}
