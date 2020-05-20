using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Scope.FileViewer.Text.ViewModels;
using Scope.Interfaces;
using Scope.Utils;

namespace Scope.FileViewer.Text
{
  [Export]
  public class FileViewerFactory : IFileViewerFactory
  {
    private readonly Dictionary<string, Func<IFile, IFileViewer>> _factories;

    public FileViewerFactory()
    {
      _factories = new Dictionary<string, Func<IFile, IFileViewer>>
                   {
                     {"json", f => new JsonTextFileViewModel(f)},
                     {"xml", f => new CryXmlTextFileViewModel(f)},
                     {"mtl", f => new CryXmlTextFileViewModel(f)},
                     {"txt", f => new TextFileViewModel(f)},
                     {"cfg", f => new TextFileViewModel(f)},
                     {"id", f => new TextFileViewModel(f)}
                   };
    }

    public bool CanHandle(IFile file)
    {
      return _factories.ContainsKey(file.Name.GetExtension());
    }

    public IFileViewer Create(IFile file)
    {
      return _factories[file.Name.GetExtension()](file);
    }
  }
}
