using System.ComponentModel.Composition;
using System.Linq;
using Scope.FileViewer.DataForge.ViewModels;
using Scope.Interfaces;

namespace Scope.FileViewer.DataForge
{
  [Export]
  public class FileViewerFactory : IFileViewerFactory
  {
    private static readonly string[] Extensions = { ".dcb" };
    private readonly IMessageQueue _messages;

    public FileViewerFactory(IMessageQueue messages)
    {
      _messages = messages;
    }

    public bool CanHandle(IFile file)
    {
      return Extensions.Any(e => file.Name.EndsWith(e));
    }

    public IFileViewer Create(IFile file)
    {
      return new DataForgeFileViewer(file, _messages);
    }
  }
}
