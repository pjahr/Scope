using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scope.Models.Interfaces;

namespace Scope.ViewModels
{
  internal class DirectoryViewModel : TreeNodeViewModel
  {
    private readonly IDirectory _directory;

    public DirectoryViewModel(IDirectory directory, TreeNodeViewModel parent) : base(parent, directory.Name)
    {
      _directory = directory;
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

      foreach (var directory in _directory.Directories)
      {
        contents.Add(new DirectoryViewModel(directory, this));
      }

      foreach (var file in _directory.Files)
      {
        contents.Add(new FileViewModel(file, this));
      }
      return contents;
    }
  }
}
