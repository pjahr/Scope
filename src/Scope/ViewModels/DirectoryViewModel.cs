using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scope.Models.Interfaces;

namespace Scope.ViewModels
{
  internal class DirectoryViewModel : TreeNodeViewModel
  {
    public DirectoryViewModel(IDirectory directory, TreeNodeViewModel parent) :
      base(parent, directory.Name)
    {
      Model = directory;
    }

    public IDirectory Model { get; }

    public override Task<List<TreeNodeViewModel>> LoadChildrenListAsync()
    {
      System.Console.WriteLine($"{Model.Name} loads children.");

      Children.Clear();

      foreach (var nodeVm in GetContents())
      {
        Children.Add(nodeVm);
      }

      return Task.FromResult(Children.ToList());
    }

    private List<TreeNodeViewModel> GetContents()
    {
      System.Console.WriteLine($"{Model.Name} retrieves children.");
      var contents = new List<TreeNodeViewModel>();

      foreach (var directory in Model.Directories)
      {
        contents.Add(new DirectoryViewModel(directory, this));
      }

      foreach (var file in Model.Files)
      {
        contents.Add(new FileViewModel(file, this));
      }

      return contents;
    }
  }
}
