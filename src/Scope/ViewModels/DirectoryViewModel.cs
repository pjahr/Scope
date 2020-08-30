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

    public DirectoryViewModel(IDirectory directory, TreeNodeViewModel parent, ISearch search, IUiDispatch uiDispatch) :
      base(parent, directory.Name, directory.Path)
    {
      Model = directory;
      _search = search;
      _uiDispatch = uiDispatch;
      _search.Finished += FilterContent;
    }


    public IDirectory Model { get; }

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

      foreach (var directory in Model.Directories)
      {
        contents.Add(new DirectoryViewModel(directory, this, _search, _uiDispatch));
      }

      foreach (var file in Model.Files)
      {
        contents.Add(new FileViewModel(file, this));
      }

      return contents;
    }

    private void FilterContent()
    {
      var contentToRemove = new List<TreeNodeViewModel>();
      foreach (var child in this.Children)
      {
        if (!ContainsOrIsAnySearchResult(child))
        {
          contentToRemove.Add(child);
        }
      }

      foreach (var content in contentToRemove)
      {
        _uiDispatch.Do(()=>Children.Remove(content));
      }
    }

    private bool ContainsOrIsAnySearchResult(TreeNodeViewModel child)
    {
      return _search.Results.Any(m => m.File.Path.StartsWith(child.Path));
    }
  }
}
