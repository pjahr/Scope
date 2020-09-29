using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Scope.Interfaces;
using Scope.Models.Interfaces;

namespace Scope.ViewModels
{
  internal class DirectoryTreeNodeViewModel : TreeNodeViewModel
  {
    private readonly ISearch _search;
    private readonly IUiDispatch _uiDispatch;

    public DirectoryTreeNodeViewModel(IDirectory directory,
                                      ISearch search,
                                      IUiDispatch uiDispatch) : base(directory.Name, directory.Path)
    {
      Model = directory;
      _search = search;
      _uiDispatch = uiDispatch;

      _search.Finished += FilterContent;
      _search.Finished += HighlightSearchTerm;
      _search.ResultsCleared += ResetName;

      if (!Model.IsEmpty)
      {
        ResetChildren();
      }
    }

    private void ResetName()
    {
      Name = Model.Name;
    }

    private void HighlightSearchTerm()
    {
      Name = GetHighlightMarkup(Name, _search.Results.Select(r => r.Term).Distinct().OrderBy(t => t.Length).ToArray());
    }

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

    public IDirectory Model { get; }

    protected override void OnDisposing()
    {
      base.OnDisposing();
      _search.Finished -= FilterContent;
    }

    public override Task<List<TreeNodeViewModel>> LoadChildrenListAsync()
    {
      Children.Clear();

      foreach (var nodeVm in GetContents())
      {
        Children.Add(nodeVm);
      }

      return Task.FromResult(Children.ToList());
    }

    private List<TreeNodeViewModel> GetContents()
    {
      var contents = new List<TreeNodeViewModel>();

      var directories = _search.Results.Any()
                                        ? Model.Directories
                                               .Where(c => _search.Results.Any(r => r.File.Path.StartsWith(c.Path)))
                                        : Model.Directories;

      var files = _search.Results.Any()
                                  ? Model.Files
                                         .Where(c => _search.Results.Any(r => r.File.Path.StartsWith(c.Path)))
                                  : Model.Files;

      foreach (var directory in directories)
      {
        contents.Add(new DirectoryTreeNodeViewModel(directory, _search, _uiDispatch));
      }

      foreach (var file in files)
      {
        contents.Add(new FileTreeNodeViewModel(file));
      }

      return contents;
    }

    private void FilterContent()
    {
      if (_search.Results.Any())
      {
        return;
      }

      var contentToRemove = Children.Where(c => !ContainsOrIsAnySearchResult(c))
                                         .ToArray();

      foreach (var content in contentToRemove)
      {
        _uiDispatch.Do(() => Children.Remove(content));
      }
    }

    private bool ContainsOrIsAnySearchResult(TreeNodeViewModel child)
    {
      return _search.Results.Any(m => m.File.Path.StartsWith(child.Path));
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
    }
  }
}
