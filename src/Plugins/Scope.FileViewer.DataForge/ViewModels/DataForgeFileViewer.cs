using System;
using System.IO;
using Scope.FileViewer.DataForge.Models;
using Scope.Interfaces;

namespace Scope.FileViewer.DataForge.ViewModels
{
  internal class DataForgeFileViewer : IFileViewer
  {
    private static DataForgeFile _df;

    internal DataForgeFileViewer(DataForgeFile df, string errorMessage)
    {
      _df = df;
      ErrorMessage = errorMessage;
    }

    public string Header => "Data Forge";
    public string ErrorMessage { get; }
    public void Dispose()
    {      
    }
  }
}
