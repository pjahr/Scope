using System;
using Scope.Interfaces;

namespace Scope.FileViewer.DataForge
{
  public class DataForgeFileViewer : IFileViewer
  {
    public DataForgeFileViewer(IFile file)
    {
      try
      {
        using (var s = file.Read())
        {
          
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
