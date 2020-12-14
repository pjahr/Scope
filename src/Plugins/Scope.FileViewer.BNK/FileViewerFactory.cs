using System.ComponentModel.Composition;
using System.Linq;
using Scope.Interfaces;

namespace Scope.FileViewer.BNK
{
  [Export]
  public class FileViewerFactory : IFileViewerFactory
  {
    private static readonly string[] Extensions;

    static FileViewerFactory()
    {
      Extensions = new[] {".bnk"};
    }

    public FileCategory Category => FileCategory.Other;

    public bool CanHandle(IFile file)
    {
      return Extensions.Any(e => file.Name.EndsWith(e));
    }

    public IFileViewer Create(IFile file)
    {
      return new BnkFileViewer(file);
    }
  }
}
