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
      _search.Began += ResetChildren;

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
      Name = ViewModelUtils.GetHighlightMarkup(Name, _search.FileResults.Select(r => r.Term).Distinct().OrderBy(t => t.Length).ToArray());
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
      var pathes = _search.FileResults.Select(r => r.File.Path).ToArray();

      foreach (var directory in GetDirectories())
      {
        contents.Add(new DirectoryTreeNodeViewModel(directory, _search, _uiDispatch));
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
      if (_search.FileResults.Any())
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
      return _search.FileResults.Any(m => m.File.Path.StartsWith(child.Path));
    }
  }
}
