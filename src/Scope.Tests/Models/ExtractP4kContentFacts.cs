using System.IO.Abstractions.TestingHelpers;
using Moq;
using Scope.Models;
using Scope.Models.Interfaces;
using Xunit;

namespace Scope.Tests.Models
{
  public class ExtractP4kContentFacts
  {
    private readonly MockFileSystem _fileSystem = new MockFileSystem();
    private readonly IOutputDirectory _outputDirectory = Mock.Of<IOutputDirectory>();
    private IExtractP4kContent _sut;
    private readonly IMessages _messages = Mock.Of<IMessages>();

    public ExtractP4kContentFacts()
    {
      _sut = new ExtractP4kContent(_fileSystem, _outputDirectory, _messages);
    }

    [Fact]
    public void It_provides_a_progress_during_extraction() { }
  }
}
