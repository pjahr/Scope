using Scope.FileViewer.WEM.Models;
using Scope.FileViewer.WEM.ViewModels;
using Scope.Interfaces;
using System.ComponentModel.Composition;
using System.Linq;

namespace Scope.FileViewer.WEM
{
  [Export]
  public class WemFileViewerFactory : IFileViewerFactory
  {
    private static readonly string[] Extensions;
    private readonly IMessageQueue _messageQueue;

    static WemFileViewerFactory()
    {
      Extensions = new[] { ".wem" };
    }

    public WemFileViewerFactory(IMessageQueue messageQueue)
    {
      _messageQueue = messageQueue;
    }

    public FileCategory Category => FileCategory.Audio;

    public bool CanHandle(IFile file)
    {
      return Extensions.Any(e => file.Name.EndsWith(e));
    }

    public IFileViewer Create(IFile file)
    {
      var model = new WemFile(file, _messageQueue);
      return new WemFileViewModel(model);
    }
  }
}
