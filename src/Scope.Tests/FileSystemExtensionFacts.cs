using Scope.Utils;
using Xunit;

namespace Scope.Tests
{
  public class FileSystemExtensionFacts
  {
    [Theory]
    [InlineData("", "")]
    [InlineData("test", "test")]
    [InlineData("test.txt", "test.txt")]
    [InlineData("/test", "test")]
    [InlineData("/test.txt", "test.txt")]
    [InlineData("/test/test.txt", "test.txt")]
    [InlineData("/test/test", "test")]
    public void GetFileName_retrieves_the_file_name_portion_of_a_path(
      string path,
      string expectedOutput)
    {
      Assert.Equal(expectedOutput, path.GetFileName());
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("test", "")]
    [InlineData("test.txt", "txt")]
    [InlineData("/test", "")]
    [InlineData("/test.txt", "txt")]
    [InlineData("/test/test.txt", "txt")]
    [InlineData(".a", "a")]
    public void GetExtension_retrieves_the_file_extension_portion_of_a_path(
      string path,
      string expectedOutput)
    {
      Assert.Equal(expectedOutput, path.GetExtension());
    }
  }
}
