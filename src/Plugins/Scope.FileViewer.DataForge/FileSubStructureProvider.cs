using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Scope.FileViewer.DataForge.Models;
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

    public async Task<IDirectory> GetAsDirectoryAsync(IFile file, IProgress<ProgressReport> progress)
    {
      var df = await _fileProvider.GetAsync(file, progress);
      var rootDirectories = df.Directories.Keys.Where(key => !key.Contains('\\'))
                                               .Select(key => df.Directories[key]);
      var rootFiles = df.Files.Keys.Where(key => !key.Contains('/'))
                                         .Select(key => df.Files[key]);

      return new Directory(file.Name, file.Path, rootDirectories, rootFiles);
    }
  }
}
