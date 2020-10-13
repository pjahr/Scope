using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scope.ViewModels
{
  internal static class ViewModelUtils
  {
    public static string GetHighlightMarkup(string text, string[] searchTerms)
    {
      var allSpans = searchTerms
                     .SelectMany(term => FindOccurrences(text, term))
                     .OrderBy(span => span.Begin).ToList();

      var distinctSpans = new List<Span>();

      while (allSpans.Any())
      {
        var start = allSpans[0];
        allSpans.Remove(start);

        var begin = start.Begin;
        var end = start.End;

        // find all spans that intersect or thouch the last span
        while (allSpans.Any() && allSpans[0].Begin <= end + 1)
        {
          if (allSpans[0].End > end)
          {
            end = allSpans[0].End;
          }
          allSpans.Remove(allSpans[0]);
        }
        distinctSpans.Add(new Span(begin, end));
      }

      var b = new StringBuilder();
      int i = 0;
      foreach (var span in distinctSpans)
      {
        b.Append(text.Substring(i, span.Begin - i));
        b.Append("├");
        b.Append(text.Substring(span.Begin, span.End - span.Begin + 1));
        b.Append("┤");
        i = span.End + 1;
      }
      b.Append(text.Substring(i, text.Length - i));

      return b.ToString();
    }

    private static IEnumerable<Span> FindOccurrences(string text, string term)
    {
      var length = term.Length;
      var offset = 0;
      while (text.Length > 0)
      {
        var i = text.IndexOf(term, StringComparison.InvariantCultureIgnoreCase);

        if (i < 0)
        {
          // no occurrence (anymore)
          yield break;
        }

        yield return new Span(offset + i, offset + i + length - 1);

        // move offset to the first character after the last occurrence
        offset = offset + i + length;

        // cut everything before the end of the last occurrence and try again
        text = text.Substring(i + length);
      }
    }

    private struct Span
    {
      public Span(int begin, int end)
      {
        Begin = begin;
        End = end;
      }
      public int Begin { get; set; }
      public int End { get; set; }
      public override string ToString()
      {
        return $"{Begin}-{End}";
      }
    }
  }
}
