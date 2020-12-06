using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Scope.FileViewer.DataForge.Models;
using Scope.FileViewer.DataForge.ViewModels;
using Scope.Interfaces;

namespace Scope.FileViewer.DataForge
{
  [Export]
  public class DataForgeFileViewerFactory : IFileViewerFactory
  {
    private static readonly string[] Extensions = { ".dcb" };
    private readonly IMessageQueue _messages;
    private readonly DataForgeFileCache _dataForgeFileCache;

    public DataForgeFileViewerFactory(IMessageQueue messages,
                                      DataForgeFileCache dataForgeFileCache)
    {
      _messages = messages;
      _dataForgeFileCache = dataForgeFileCache;
    }

    public bool CanHandle(IFile file)
    {
      return Extensions.Any(e => file.Name.EndsWith(e));
    }

    public IFileViewer Create(IFile file)
    {
      var errorMessage = "";

      if (_dataForgeFileCache[file] == null)
      {
        // HACK(PJ): load only once for now. Caching for open p4k most likely. 
        try
        {
          using (var s = file.Read())
          using (var r = new BinaryReader(s))
          {
            _dataForgeFileCache[file] = new DataForgeFile(r, _messages);
          }
        }
        catch (Exception e)
        {
          errorMessage =
            $"There was an error opening {file.Name}.\r\n\r\n{e.Message}\r\n\r\n{e.StackTrace}";

          _messages.Add(errorMessage);
        }
      }

      return new DataForgeFileViewer(_dataForgeFileCache[file], errorMessage);
    }
  }
}
