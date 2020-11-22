using System.Collections.Generic;
using System.IO;
using System.Linq;
using Scope.Interfaces;
using Scope.Models;
using Scope.Models.Interfaces;
using Scope.Zip.Zip;
using Xunit;

namespace Scope.Tests.Models
{
  public class GenerateFileSystemFacts
  {
    private GenerateFileSystem _sut;
    private IFileSystem _result;
    private ZipFile _zipFile;
    private IDictionary<string, int> _fileTypes = new Dictionary<string, int>();

    public GenerateFileSystemFacts()
    {
      GivenItIsCreated();
    }

    [Fact]
    public void It_creates_a_root_directory()
    {
      using (var stream = new MemoryStream())
      {
        _zipFile = new ZipFile(stream);
      }

      WhenItGenerates();

      Assert.NotNull(_result.Root);
      ThenTheNumberOfDirectoriesInRootIs(0);
      ThenTheNumberOfFilesInRootIs(0);
    }

    [Fact]
    public void It_creates_a_file_in_root()
    {
      const string fileName = "test.txt";
      const string fileContent = "file content";

      using (var stream = new MemoryStream())
      {
        using (var data = new FakeFileData(fileContent))
        {
          _zipFile = new ZipFile(stream);
          _zipFile.BeginUpdate();
          _zipFile.Add(data, fileName);
          _zipFile.CommitUpdate();
        }
      }

      WhenItGenerates();

      Assert.NotNull(_result.Root);
      ThenTheNumberOfDirectoriesInRootIs(0);
      ThenTheNumberOfFilesInRootIs(1);
      ThenRootContainsFile(fileName);
      ThenFileContains(fileName, fileContent);
    }

    [Fact]
    public void It_creates_a_file_in_a_subdirectory()
    {
      const string filePath = "first/second/test.txt";
      const string fileName = "test.txt";
      const string fileContent = "file content";

      using (var stream = new MemoryStream())
      {
        using (var data = new FakeFileData(fileContent))
        {
          _zipFile = new ZipFile(stream);
          _zipFile.BeginUpdate();
          _zipFile.Add(data, filePath);
          _zipFile.CommitUpdate();
        }
      }

      WhenItGenerates();

      ThenTheNumberOfDirectoriesInRootIs(1);
      ThenTheNumberOfFilesInRootIs(0);
      ThenRootContainsFile("first", "second", fileName);
      ThenFileContains(filePath, fileContent);
      ThenFileTypesContains("txt", 1);
    }

    private void GivenItIsCreated()
    {
      _sut = new GenerateFileSystem();
    }

    private void WhenItGenerates()
    {
      _result = _sut.Generate(_zipFile, _fileTypes);
    }

    private void ThenTheNumberOfDirectoriesInRootIs(int expected)
    {
      Assert.Equal(expected, _result.Root.Directories.Count);
    }

    private void ThenTheNumberOfFilesInRootIs(int expected)
    {
      Assert.Equal(expected, _result.Root.Files.Count);
    }

    private void ThenRootContainsFile(params string[] pathSegments)
    {
      IDirectory current = _result.Root;

      for (int i = 0; i < pathSegments.Length - 1; i++)
      {
        current = current.Directories.Single(d => d.Name == pathSegments[i]);
      }

      Assert.NotNull(current.Files.SingleOrDefault(f => f.Name
                                                        == pathSegments[pathSegments.Length - 1]));
    }

    private void ThenFileContains(string fileName, string fileContent) 
    {
      
    }
    private void ThenFileTypesContains(string extension, int count) 
    {
      Assert.Equal(count, _fileTypes[extension]);
    }
  }
}
