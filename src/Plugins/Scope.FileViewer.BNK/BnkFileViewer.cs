using System;
using System.IO;
using Scope.Interfaces;

namespace Scope.FileViewer.BNK
{
  public class BnkFileViewer : IFileViewer
  {
    private readonly IFile _file;

    public BnkFileViewer(IFile file)
    {
      try
      {
        var bytes = GetRawBytes();

        var bkhd = Convert.ToChar(bytes[0]);
        

      }
      catch (Exception e)
      {
        ErrorMessage =
          $"There was an error opening {file.Name}.\r\n\r\n{e.Message}\r\n\r\n{e.StackTrace}";
      }
      _file = file;
    }

    public string Header => "Wwise Sound Bank (BNK)";
    public string ErrorMessage { get; }

    public void Dispose()
    {
      
    }

    private byte[] GetRawBytes()
    {
      using (var stream = _file.Read())
      using (var memoryStream = new MemoryStream())
      {
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
      }
    }
  }
}
