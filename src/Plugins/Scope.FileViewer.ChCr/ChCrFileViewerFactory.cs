using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Scope.FileViewer.ChCr;
using Scope.Interfaces;
using Scope.Utils;

namespace Scope.FileViewer.Text
{
  [Export]
  public class ChCrFileViewerFactory : IFileViewerFactory
  {
    private readonly Dictionary<string, Func<IFile, IFileViewer>> _factories;

    public ChCrFileViewerFactory()
    {
      _factories = new Dictionary<string, Func<IFile, IFileViewer>>
                   {
                     {"pla", f => new ChCrFileViewModel(f)},
                   };
    }

    public FileCategory Category => FileCategory.Text;

    public bool CanHandle(IFile file)
    {
      return _factories.ContainsKey(file.Name.GetExtension());
    }    

    public Task<IFileViewer> CreateAsync(IFile file, IProgress<ProgressReport> progress)
    {
      return Task.FromResult(_factories[file.Name.GetExtension().ToLower()](file));
    }
  }
}
