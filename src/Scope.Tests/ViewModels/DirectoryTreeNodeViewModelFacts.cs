using Scope.ViewModels;
using Xunit;

namespace Scope.Tests.ViewModels
{
  public class DirectoryTreeNodeViewModelFacts
  {
    [Theory]
    //          terms, name,  expected
    [InlineData("",    "",    "")]
    [InlineData("a",   "a", "├a┤")]
    [InlineData("a",   "b",   "b")]
    [InlineData("a b", "a", "├a┤")]
    [InlineData("a b", "ab", "├ab┤")]
    [InlineData("a",   "a a", "├a┤ ├a┤")]
    [InlineData("text", "Some text containing TEXT.", "Some ├text┤ containing ├TEXT┤.")]
    [InlineData("text", "Some string containing characters", "Some string containing characters")]
    [InlineData("ng in", "Engine", "E├ngin┤e")]
    public void It_can_build_the_search_term_markup(string searchTerms,
                                                    string name,
                                                    string expected)
    {
      var terms = searchTerms.Split(' ');
      Assert.Equal(expected, DirectoryTreeNodeViewModel.GetHighlightMarkup(name, terms));
    }
  }
}
