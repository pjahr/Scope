using Scope.Interfaces;
using System;
using System.ComponentModel.Composition;
using System.Windows;

namespace Scope.FileViewer.DataForge
{
  [Export]
  public class PluginResources : ResourceDictionary, IResourceDictionary
  {
    public Uri Uri { get; } = ExternalResourceDictionaryUri.Create<PluginResources>();
  }
}
