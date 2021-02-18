using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Scope.Interfaces;
using Scope.Utils;

namespace Scope.FileViewer.DDS
{
  [Export]
  public class DdsFileViewerFactory : IFileViewerFactory
  {
    private static readonly string[] Extensions;
    private readonly IFileService _fileService;

    static DdsFileViewerFactory()
    {
      Extensions = new[] { ".dds", ".1", ".2", ".3", ".4", ".5", ".6", ".7", ".8", ".a", ".1a", ".2a", ".3a", ".4a", ".5a", ".6a", ".7a", ".8a" };
    }

    public DdsFileViewerFactory(IFileService fileService)
    {
      _fileService = fileService;
    }

    public FileCategory Category => FileCategory.Image;

    public bool CanHandle(IFile file)
    {
      return Extensions.Any(e => file.Name.EndsWith(e));
    }

    public Task<IFileViewer> CreateAsync(IFile file, System.IProgress<ProgressReport> progress)
    {
      IFileViewer f;

      if (file.GetExtension() == "dds")
      {
        f = new DdsFileViewer(file);
        return Task.FromResult(f);
      }

      var baseFilePath = file.Path.Substring(0, file.Path.LastIndexOf('.'));

      var baseFile = _fileService.Get(baseFilePath);
      f = new DdsFileViewer(new MergedDdsFile(file, baseFile));
      return Task.FromResult(f);
    }
  }
}
