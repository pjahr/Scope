using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Scope.Interfaces;

namespace Scope.FileViewer.BNK
{
  [Export]
  public class FileViewerFactory : IFileViewerFactory
  {
    private static readonly string[] Extensions;

    static FileViewerFactory()
    {
      Extensions = new[] { ".bnk" };
    }

    public FileCategory Category => FileCategory.Other;

    public bool CanHandle(IFile file)
    {
      return Extensions.Any(e => file.Name.EndsWith(e));
    }

    public Task<IFileViewer> CreateAsync(IFile file, IProgress<ProgressReport> progress)
    {
      return Task.FromResult((IFileViewer)new BnkFileViewer(file));
    }
  }
}
