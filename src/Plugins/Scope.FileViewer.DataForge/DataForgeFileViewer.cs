using System;
using System.IO;
using Scope.Interfaces;

namespace Scope.FileViewer.DataForge
{
  public class DataForgeFileViewer : IFileViewer
  {
    private DataForgeFile _df;

    public DataForgeFileViewer(IFile file)
    {
      try
      {
        using (var s = file.Read())
        using (var r = new BinaryReader(s))
        {
          _df= new DataForgeFile(r);
        }
      }
      catch (Exception e)
      {
        ErrorMessage =
          $"There was an error opening {file.Name}.\r\n\r\n{e.Message}\r\n\r\n{e.StackTrace}";
      }
    }

    public string Header => "Data Forge";
    public string ErrorMessage { get; }
    public void Dispose()
    {
      
    }
  }
}
