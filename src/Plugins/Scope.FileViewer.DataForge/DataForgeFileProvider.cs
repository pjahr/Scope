using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
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

    public Task<DataForgeFile?> GetAsync(IFile file, IProgress<ProgressReport> progress)
    {
      if (_currentFile == file)
      {
        return Task.FromResult(_current);
      }

      return Task.Run(() =>
      {
        // HACK(PJ): load only once for now. Caching for open p4k most likely. 
        try
        {
          using var s = file.Read();
          using var r = new BinaryReader(s);

          _currentFile = file;
          _current = new DataForgeFile(r, _messages, progress);

          return _current;
        }
        catch (Exception e)
        {
          var errorMessage = $"There was an error opening {file.Name}.\r\n\r\n{e.Message}\r\n\r\n{e.StackTrace}";
          progress.Report(new ProgressReport(0.0, errorMessage));

          _messages.Add(errorMessage);

          return null;
        }
      });
    }
  }
}
