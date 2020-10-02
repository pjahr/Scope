using System.Collections.Generic;
using System.ComponentModel.Composition;
using Scope.Interfaces;

namespace Scope.FileViewer.Text
{
  [Export]
  public class SearchProvider : ISearchProvider
  {
    public SearchProvider()
    {
      FileTypes = new[]
      {
        new SeachableFileType("txt", "Text"),
        new SeachableFileType("json"),
        new SeachableFileType("xml"),
        new SeachableFileType("mtl"),
        new SeachableFileType("cfg"),
        new SeachableFileType("cfgm"),
        new SeachableFileType("cfgf"),
        new SeachableFileType("ini"),
        new SeachableFileType("id"),
      };
    }   

    public IEnumerable<ISearchableFileType> FileTypes { get; }
  }
}
