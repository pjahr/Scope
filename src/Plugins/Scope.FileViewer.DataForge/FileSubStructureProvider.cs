using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Scope.FileViewer.DataForge.ViewModels;
using Scope.Interfaces;

namespace Scope.FileViewer.DataForge
{
  [Export]
  internal class FileSubStructureProvider : IFileSubStructureProvider
  {
    private readonly DataForgeFileCache _dataForgeFileCache;

    public string ApplicableFileExtension => ".dcb";

    public FileSubStructureProvider(DataForgeFileCache dataForgeFileCache)
    {
      _dataForgeFileCache = dataForgeFileCache;
    }

    public IReadOnlyCollection<IDirectory> GetDirectories(IFile file)
    {
      var df = _dataForgeFileCache[file];
      var rootDirectories = df.Directories.Keys.Where(key => !key.Contains('\\'))
                                               .Select(key => df.Directories[key]);

      return new List<IDirectory>(rootDirectories);
    }

    public IReadOnlyCollection<IFile> GetFiles(IFile file)
    {
      var df = _dataForgeFileCache[file];
      var rootDirectories = df.Files.Keys.Where(key => !key.Contains('/'))
                                         .Select(key => df.Files[key]);

      return new List<IFile>(rootDirectories);
    }
  }
}
