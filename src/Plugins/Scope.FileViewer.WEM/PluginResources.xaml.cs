using System;
using System.ComponentModel.Composition;
using System.Windows;
using Scope.Interfaces;

namespace Scope.FileViewer.WEM
{
  [Export]
  public class PluginResources : ResourceDictionary, IResourceDictionary
  {
    public Uri Uri { get; } = ExternalResourceDictionaryUri.Create<PluginResources>();
  }
}
