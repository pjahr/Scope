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

      AssertExistenceOfDirectory(outputDirectoryPath);

      using (var s = file.Read())
      using (var f=_fileSystem.File.Create(outputPath))
      {
        s.CopyTo(f);
      }
    }

    private void AssertExistenceOfDirectory(string outputDirectoryPath)
    {
      _fileSystem.Directory.CreateDirectory(outputDirectoryPath);
    }

    public void Extract(IDirectory directory, string outputDirectoryPath)
    {
      var outputPath = Path.Combine(outputDirectoryPath, directory.Name);

      foreach (var childDirectory in directory.Directories)
      {
        Extract(childDirectory, outputPath);
      }

      foreach (var file in directory.Files)
      {
        Extract(file, outputPath);
      }
    }
  }
}
