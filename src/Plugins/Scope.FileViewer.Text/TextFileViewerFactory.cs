using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Text;
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
                     {"json", CreateJsonFileViewer},
                     {"xml", CreateMatchingViewerBasedOnContent},
                     {"pla", CreateChCrFileViewer},
                     {"soc", CreateChCrFileViewer},
                     {"ale", CreateChCrFileViewer},
                     {"mtl", CreateCryXmlFileViewer},
                     {"entxml", CreateCryXmlFileViewer},
                     {"rmp", CreateCryXmlFileViewer},
                     {"txt", CreateTextFileViewer},
                     {"cfg", CreateTextFileViewer},
                     {"cfgf", CreateTextFileViewer},
                     {"cfgm", CreateTextFileViewer},
                     {"ini", CreateTextFileViewer},
                     {"id", CreateTextFileViewer}
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

    private IFileViewer CreateTextFileViewer(IFile file) => new TextFileViewModel(file);
    private IFileViewer CreateCryXmlFileViewer(IFile file) => new CryXmlTextFileViewModel(file);
    private IFileViewer CreateJsonFileViewer(IFile file) => new JsonTextFileViewModel(file);
    private IFileViewer CreateChCrFileViewer(IFile file) => new ChCrTextFileViewModel(file);

    private IFileViewer CreateMatchingViewerBasedOnContent(IFile file)
    {
      string header = "";
      var first10Bytes = new byte[10];
      using (var stream = file.Read())
      {
        stream.Read(first10Bytes, 0, 10);
        header = Encoding.UTF8.GetString(first10Bytes);
      }

      if (header.StartsWith("CryXmlB"))
      {
        return CreateCryXmlFileViewer(file);
      }

      if (header.StartsWith("CrChF"))
      {
        return CreateChCrFileViewer(file);
      }

      Debug.WriteLine($"Unknown header: {header}");

      // if unknown try simple UTF8 text
      return CreateTextFileViewer(file);

    }
  }
}
