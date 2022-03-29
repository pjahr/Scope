using System;
using System.ComponentModel.Composition;
using System.IO;
using Scope.Interfaces;
using Scope.Models.Interfaces;
using IFileSystem = System.IO.Abstractions.IFileSystem;

namespace Scope.Models
{
  [Export]
  internal class ExtractP4kContent : IExtractP4kContent
  {
    private readonly IFileSystem _fileSystem;
    private readonly IOutputDirectory _outputDirectory;
    private readonly IMessageQueue _messages;

    public ExtractP4kContent(IFileSystem fileSystem, 
                             IOutputDirectory outputDirectory,
                             IMessageQueue messages)
    {
      _fileSystem = fileSystem;
      _outputDirectory = outputDirectory;
      _messages = messages;
    }

    public void Extract(IFile file,
                        string outputDirectoryPath)
    {
      if (string.IsNullOrEmpty(outputDirectoryPath))
      {
        outputDirectoryPath = _outputDirectory.Path;
      }

      var outputPath = Path.Combine(outputDirectoryPath, file.Name);

      try
      {
        AssertExistenceOfDirectory(outputDirectoryPath);

        using (var s = file.Read())
        using (var f = _fileSystem.File.Create(outputPath))
        {
          s.CopyTo(f);
        }
      }
      catch (Exception e)
      {
        _messages.Add($"Extraction failed: {file.Path} --> {outputPath}");
        _messages.Add(e.Message);
        _messages.Add(e.StackTrace);
      }

      _messages.Add($"Extracted: {file.Path} --> {outputPath}");
    }

    private void AssertExistenceOfDirectory(string outputDirectoryPath)
    {
      _fileSystem.Directory.CreateDirectory(outputDirectoryPath);
    }

    public void Extract(IDirectory directory, string outputDirectoryPath)
    {
      if (outputDirectoryPath == "")
      {
        outputDirectoryPath = _outputDirectory.Path;
      }

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
