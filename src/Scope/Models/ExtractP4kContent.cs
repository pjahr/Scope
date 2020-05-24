using Scope.Interfaces;
using Scope.Models.Interfaces;
using System.ComponentModel.Composition;
using System.IO;

namespace Scope.Models
{
  [Export]
  internal class ExtractP4kContent : IExtractP4kContent
  {
    private readonly System.IO.Abstractions.IFileSystem _fileSystem;

    public ExtractP4kContent(System.IO.Abstractions.IFileSystem fileSystem)
    {
      _fileSystem = fileSystem;
    }

    public void Extract(IFile file, string outputDirectoryPath)
    {
      var outputPath = Path.Combine(outputDirectoryPath, file.Name);

      using (var s = file.Read())
      using (var f=_fileSystem.File.Create(outputPath))
      {
        s.CopyTo(f);
      }
    }

    public void Extract(IDirectory directory, string outputDirectoryPath)
    {
      var outputPath = Path.Combine(outputDirectoryPath, directory.Name);

      foreach (var subdirectory in directory.Directories)
      {
        Extract(subdirectory, outputPath);
      }

      foreach (var file in directory.Files)
      {
        Extract(file, outputPath);
      }
    }
  }
}
