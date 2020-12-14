using Scope.Interfaces;
using Scope.Models.Interfaces;
using System.ComponentModel.Composition;

namespace Scope.Models
{
  [Export]
  internal class FileService : IFileService
  {
    private readonly ICurrentP4k _currentP4k;

    public FileService(ICurrentP4k currentP4k)
    {
      _currentP4k = currentP4k;
    }

    public IFile? Get(string path)
    {
      if (_currentP4k == null)
      {
        return null;
      }

      return _currentP4k.FileSystem.GetFile(path);
    }
  }
}
