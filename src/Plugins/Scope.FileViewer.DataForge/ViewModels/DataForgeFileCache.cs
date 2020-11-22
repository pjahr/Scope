using Scope.FileViewer.DataForge.Models;
using System.ComponentModel.Composition;

namespace Scope.FileViewer.DataForge.ViewModels
{
  [Export]
  public class DataForgeFileCache
  {
    public DataForgeFile Current { get; set; }
  }
}
