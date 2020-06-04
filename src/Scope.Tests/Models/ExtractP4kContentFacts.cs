using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Moq;
using Scope.Models;
using Scope.Models.Interfaces;
using Xunit;

namespace Scope.Tests.Models
{
    public class ExtractP4kContentFacts
    {
      private MockFileSystem _fileSystem;
      private IOutputDirectory _outputDirectory;
      private IExtractP4kContent _sut;

      [Fact]
      public void It_provides_a_progress_during_extraction()
      {
        _fileSystem = new MockFileSystem();
        _outputDirectory = Mock.Of<IOutputDirectory>();

        _sut= new ExtractP4kContent(_fileSystem, _outputDirectory);
      }
    }
}
