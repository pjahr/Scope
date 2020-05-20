using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Scope.Interfaces;

namespace Scope.ViewModels
{
  internal class FileViewModel : TreeNodeViewModel
  {
    private readonly IFile _file;

    public FileViewModel(IFile file, TreeNodeViewModel parent) : base(parent, file.Name)
    {
      _file = file;
    }

    public IFile File => _file;

    public override bool HasDummyChild => false;

    protected override void LoadChildren() { }

    protected override void ResetChildren() { }

    public override Task<List<TreeNodeViewModel>> LoadChildrenListAsync()
    {
      throw new NotImplementedException();
    }
  }
}
