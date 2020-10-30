using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scope.Interfaces;
using Scope.Models.Interfaces;

namespace Scope.ViewModels
{
  internal class DirectoryTreeNodeViewModel : TreeNodeViewModel
  {
    private readonly ISearch _search;
    private readonly ISearchOptions _searchOptions;
    private readonly IUiDispatch _uiDispatch;

    public DirectoryTreeNodeViewModel(IDirectory directory,
                                      ISearch search,
                                      ISearchOptions searchOptions,
                                      IUiDispatch uiDispatch) : base(directory.Name, directory.Path)
    {
      Model = directory;
      _search = search;
      _searchOptions = searchOptions;
      _uiDispatch = uiDispatch;

      _search.Finished += FilterContent;
      _search.Finished += HighlightSearchTerm;
      _search.ResultsCleared += ResetName;
      _search.Began += ResetChildren;

      //if (!Model.IsEmpty)
      //{
      //  ResetChildren();
      //}
      HighlightSearchTerm();
    }

    private void ResetName()
    {
      Name = Model.Name;
    }

    private void HighlightSearchTerm()
    {
      var searchTerms = _search.CurrentSearchTerms
                                                                  .Distinct()
                                                                  .OrderBy(t => t.Length)
                                                                  .ToArray();
      if (!searchTerms.Any())
      {
        searchTerms = new[] { "" };
      }

      Console.WriteLine($"{Model.Name} <- {searchTerms.Aggregate((c,n)=>$"{c},{n}")}");

      Name = ViewModelUtils.GetHighlightMarkup(Name, searchTerms);
    }    

    public IDirectory Model { get; }

    protected override void OnDisposing()
    {
      base.OnDisposing();
      _search.Finished -= FilterContent;
      _search.Finished -= HighlightSearchTerm;
      _search.ResultsCleared -= ResetName;
      _search.Began -= ResetChildren;
    }

    public override Task<List<TreeNodeViewModel>> LoadChildrenListAsync()
    {
      foreach (var item in Children)
      {
        item.Dispose();
      }
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
      var pathes = _search.FileResults.Select(r => r.File.Path).ToArray();

      foreach (var directory in GetDirectories())
      {
        contents.Add(new DirectoryTreeNodeViewModel(directory, _search, _searchOptions, _uiDispatch));
      }

      foreach (var file in GetFiles())
      {
        contents.Add(new FileTreeNodeViewModel(file, _search));
      }

      return contents;
    }

    private IEnumerable<IFile> GetFiles()
    {
      if (!_search.FileResults.Any())
      {
        return Model.Files;
      }

      return Model.Files.Where(f => _search.ResultIds.Contains(f.Index));
    }

    private IEnumerable<IDirectory> GetDirectories()
    {
      return _search.FileResults.Any()
               ? Model.Directories.Where(d => _search.ResultPaths.Any(path => path.StartsWith(d.Path)))
               : Model.Directories;
    }

    private void FilterContent()
    {
      switch (_searchOptions.Mode)
      {
        case SearchMode.FileName:
          RemoveContentsThatDoNotMatchSearchTerm();
          return;
        case SearchMode.FileContent:
          break;
        case SearchMode.FileNameAndContent:
          break;
        default:
          break;
      }      
    }

    private bool IsAFile(TreeNodeViewModel c)
    {
      return c is FileTreeNodeViewModel;
    }

    private void RemoveContentsThatDoNotMatchSearchTerm()
    {
      if (!_search.FileResults.Any())
      {
        return;
      }

      var contentsToRemove = Children.Where(c => !ContainsOrIsAnyFileSearchResult(c))
                                     .ToArray();

      foreach (var content in contentsToRemove)
      {
        _uiDispatch.Do(() => Children.Remove(content));
      }
    }

    private bool ContainsOrIsAnyFileSearchResult(TreeNodeViewModel child)
    {
      return _search.FileResults.Any(m => m.File.Path.StartsWith(child.Path));
    }

    public override string ToString()
    {
      return $"{Name} ({Path})";
    }
  }
}
