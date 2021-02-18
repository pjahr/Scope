using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Scope.FileViewer.Text.ViewModels;
using Scope.Interfaces;
using Scope.Utils;

namespace Scope.FileViewer.Text
{
  [Export]
  public class TextFileViewerFactory : IFileViewerFactory
  {
    private readonly Dictionary<string, Func<IFile, IFileViewer>> _factories;

    public TextFileViewerFactory()
    {
      _factories = new Dictionary<string, Func<IFile, IFileViewer>>
                   {
                     {"json", f => new JsonTextFileViewModel(f)},
                     {"xml", f => new CryXmlTextFileViewModel(f)},
                     {"mtl", f => new CryXmlTextFileViewModel(f)},
                     {"txt", f => new TextFileViewModel(f)},
                     {"cfg", f => new TextFileViewModel(f)},
                     {"cfgf", f => new TextFileViewModel(f)},
                     {"cfgm", f => new TextFileViewModel(f)},
                     {"ini", f => new TextFileViewModel(f)},
                     {"id", f => new TextFileViewModel(f)}
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
