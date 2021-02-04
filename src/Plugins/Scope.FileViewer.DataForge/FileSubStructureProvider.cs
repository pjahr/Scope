using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Scope.Interfaces;

namespace Scope.FileViewer.DataForge
{
  [Export]
  public class FileSubStructureProvider : IFileSubStructureProvider
  {
    private readonly DataForgeFileProvider _fileProvider;

    public string ApplicableFileExtension => ".dcb";

    public FileSubStructureProvider(DataForgeFileProvider dataForgeFileProvider)
    {
      _fileProvider = dataForgeFileProvider;
    }

    public IReadOnlyCollection<IDirectory> GetDirectories(IFile file)
    {
      var df = _fileProvider.Get(file, out string _);
      var rootDirectories = df.Directories.Keys.Where(key => !key.Contains('\\'))
                                               .Select(key => df.Directories[key]);

      return new List<IDirectory>(rootDirectories);
    }

    public IReadOnlyCollection<IFile> GetFiles(IFile file)
    {
      var df = _fileProvider.Get(file, out string _);
      var rootDirectories = df.Files.Keys.Where(key => !key.Contains('/'))
                                         .Select(key => df.Files[key]);

      return new List<IFile>(rootDirectories);
    }
  }
}
