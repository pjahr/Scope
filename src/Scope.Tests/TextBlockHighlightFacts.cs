using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Scope.Tests
{
  public class TextBlockHighlightFacts
  {
    [Theory]
    [InlineData("",       "")]
    [InlineData("[]",     "")]
    [InlineData("[test]", "test")]
    [InlineData("[b]a",   "b_")]
    [InlineData("a[b]",   "_b")]
    [InlineData("a[b]a",  "_b_")]
    [InlineData("[b]a[b]","b_b")]
    public void It_highlights_text_in_square_brackets(string term, string expected)
    {

      string[] runs = term.Split(new string[] { "[", "]" }, StringSplitOptions.None);
      List<string> actual = new List<string>();

      for (int i = 0; i < runs.Length; i++)
      {
        var run = i % 2 == 1 ? runs[i] : new string(runs[i].Select(_ => '_').ToArray());

        actual.Add(run);
      }

      Assert.Equal(expected, actual.Aggregate((c, n) => $"{c}{n}"));

    }
  }
}
