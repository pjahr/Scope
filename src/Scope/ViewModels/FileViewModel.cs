using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Scope.Interfaces;

namespace Scope.ViewModels
{
  internal class FileViewModel : TreeNodeViewModel
  {
    public FileViewModel(IFile file, TreeNodeViewModel parent) : base(parent, file.Name)
    {
      Model = file;
    }

    public IFile Model { get; }
    public override bool HasDummyChild => false;

    protected override void LoadChildren() { }

    protected override void ResetChildren() { }

    public override Task<List<TreeNodeViewModel>> LoadChildrenListAsync()
    {
      throw new NotImplementedException();
    }
  }
}
