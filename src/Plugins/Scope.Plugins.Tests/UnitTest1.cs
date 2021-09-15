using Moq;
using Scope.FileViewer.DataForge.Models;
using Scope.Interfaces;
using System;
using System.IO;
using Xunit;
using File = System.IO.File;

namespace Scope.Plugins.Tests
{
  public class DataForgeFileFacts
  {
    private DataForgeFile _sut;
    private IMessageQueue _messageQueue;
    private IProgress<ProgressReport> _progress;

    [Fact(Skip ="Run on demand")]
    public void It_deserializes_the_dcb_contents()
    {
      _messageQueue = Mock.Of<IMessageQueue>();
      _progress = Mock.Of<IProgress<ProgressReport>>();

      using var r = new BinaryReader(File.OpenRead(@"TestData/game.dcb"));
      _sut = new DataForgeFile(r,_messageQueue, _progress);

      
    }
  }
}
