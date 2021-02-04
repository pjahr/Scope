using System;
using System.ComponentModel.Composition;
using System.IO;
using Scope.FileViewer.DataForge.Models;
using Scope.Interfaces;

namespace Scope.FileViewer.DataForge
{
  [Export]
  public class DataForgeFileProvider
  {
    private readonly IMessageQueue _messages;
    IFile _currentFile;
    DataForgeFile _current;

    public DataForgeFileProvider(IMessageQueue messages)
    {
      _messages = messages;
    }

    public DataForgeFile? Get(IFile file, out string errorMessage)
    {
      errorMessage = "";

      if (_currentFile == file)
      {
        return _current;
      }

      // HACK(PJ): load only once for now. Caching for open p4k most likely. 
      try
      {
        using (var s = file.Read())
        using (var r = new BinaryReader(s))
        {
          _currentFile = file;
          _current = new DataForgeFile(r, _messages);
          return _current;
        }
      }
      catch (Exception e)
      {
        errorMessage =
          $"There was an error opening {file.Name}.\r\n\r\n{e.Message}\r\n\r\n{e.StackTrace}";

        _messages.Add(errorMessage);
        return null;
      }
    }
  }
}
