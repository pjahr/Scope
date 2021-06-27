using Scope.Deserialization;
using System.IO;
using Xunit;

namespace Scope.Tests.Deserialization
{
  public class CryXmlDeserializerFacts
  {
    [Fact]
    public void It_()
    {
      var file = Properties.Resources.battleroyale;

      using var s = new MemoryStream(file);

      var xdoc = CryXmlSerializer.ReadStream(s);
    }
  }
}
