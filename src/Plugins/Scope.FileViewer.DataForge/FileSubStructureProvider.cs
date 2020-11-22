using System.Collections.Generic;
using System.ComponentModel.Composition;
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
      throw new System.NotImplementedException();
    }

    public IReadOnlyCollection<IFile> GetFiles(IFile file)
    {
      throw new System.NotImplementedException();
    }
  }
}
