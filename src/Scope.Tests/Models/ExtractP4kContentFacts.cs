using System.IO;
using System.IO.Abstractions.TestingHelpers;
using Moq;
using Scope.Interfaces;
using Scope.Models;
using Scope.Models.Interfaces;
using Xunit;

namespace Scope.Tests.Models
{
  public class ExtractP4kContentFacts
  {
    private const string DefaultPath = @"default\path";
    private readonly MockFileSystem _fileSystem = new MockFileSystem();
    private readonly IOutputDirectory _outputDirectory = Mock.Of<IOutputDirectory>();
    private IExtractP4kContent _sut;
    private readonly IMessageQueue _messages = Mock.Of<IMessageQueue>();

    public ExtractP4kContentFacts()
    {
      _outputDirectory.ReturnsOn(m => m.Path, DefaultPath);

      _sut = new ExtractP4kContent(_fileSystem, _outputDirectory, _messages);
    }

    [Fact]
    public void It_reads_the_given_file()
    {
      var bytes = new byte[] { 1, 2, 3 };

      var file = Mock.Of<IFile>().ReturnsOn(m => m.Name, "some.file")
                                 .ReturnsOn(m => m.Read(), new MemoryStream(bytes));

      _fileSystem.AddDirectory(DefaultPath);

      _sut.Extract(file, null);

      file.Mock().Verify(m => m.Read(), Times.Once);

      Assert.Equal(bytes, _fileSystem.GetFile(@"default\path\some.file").Contents);
    }

    [Fact]
    public void If_the_given_output_path_is_empty_it_uses_the_default_output_directory()
    {
      
    }

    [Fact]
    public void It_provides_a_progress_during_extraction()
    {
      
    }
  }
}
