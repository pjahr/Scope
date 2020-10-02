using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Scope.Interfaces;
using Scope.Models.Interfaces;
using Scope.Utils;

namespace Scope.Models
{
  /// <summary>
  /// Provides available file types (primarily for search features).
  /// Available file types depend on the contents of the currently loaded p4k file.
  /// Searchable file types depend on available search extensions.
  /// </summary>
  [Export]
  internal class KnownFileExtensions : IKnownFileExtensions
  {
    private readonly ICurrentP4k _currentP4K;
    private readonly ISearchableFileType[] _searchableFileTypes;

    public KnownFileExtensions(ICurrentP4k currentP4K,
                               IEnumerable<ISearchProvider> searchProviders = null,
                               IEnumerable<ISearchableFileType> searchableFileTypes = null)
    {
      _currentP4K = currentP4K;

      _searchableFileTypes = searchProviders.EmptyWhenNull()
                                            .SelectMany(p => p.FileTypes)
                                            .Concat(searchableFileTypes.EmptyWhenNull())
                                            .ToArray();
      
      _currentP4K.Changed += Update;

      Update();
    }

    public event Action Changed;

    private void Update()
    {
      All = _currentP4K.Statistics.FileTypes.Keys.Select(k=>k.ToLower()).ToArray();

      var seachableExtensions = _searchableFileTypes.Select(t => t.Extension.ToLower()).ToArray();

      Searchable = _currentP4K.Statistics.FileTypes.Keys.Where(seachableExtensions.Contains).ToArray();

      Changed.Raise();
    }

    public IEnumerable<string> All { get; private set; }
    public IEnumerable<string> Searchable { get; private set; }
  }
}
