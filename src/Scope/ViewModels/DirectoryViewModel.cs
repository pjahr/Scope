using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scope.Interfaces;
using Scope.Models;
using Scope.Models.Interfaces;

namespace Scope.ViewModels
{
  internal class DirectoryViewModel : TreeNodeViewModel
  {
    private readonly ISearch _search;
    private readonly IUiDispatch _uiDispatch;

    public DirectoryViewModel(IDirectory directory,
                              TreeNodeViewModel parent,
                              ISearch search,
                              IUiDispatch uiDispatch) : base(parent, directory.Name, directory.Path)
    {
      Model = directory;
      _search = search;
      _uiDispatch = uiDispatch;

      _search.Finished += FilterContent;

      if (!Model.IsEmpty)
      {
        ResetChildren();
      }
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
        contents.Add(new DirectoryViewModel(directory, this, _search, _uiDispatch));
      }

      foreach (var file in files)
      {
        contents.Add(new FileViewModel(file, this));
      }

      return contents;
    }

    private void FilterContent()
    {
      if (_search.Results.Any())
      {
        return;
      }

      var contentToRemove = this.Children.Where(c => !ContainsOrIsAnySearchResult(c))
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
