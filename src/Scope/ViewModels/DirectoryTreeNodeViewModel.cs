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

    public static string GetHighlightMarkup(string text, string [] searchTerms)
    {
      var terms = searchTerms.Distinct().OrderBy(t => t.Length).ToArray();

      var b = new StringBuilder();
      foreach (var term in terms.Where(t=>text.Contains(t)))
      {
        int i = 0;
        while (i < text.Length)
        {
          var j = text.IndexOf(term,StringComparison.InvariantCultureIgnoreCase);
          var textBefore = text.Substring(i, j - i);
          var match = text.Substring(j, term.Length);
          b.Append(textBefore);
          b.Append($"[{match}]");
          i = i + textBefore.Length + match.Length;
        }
      }

      return "";
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
  }
}
