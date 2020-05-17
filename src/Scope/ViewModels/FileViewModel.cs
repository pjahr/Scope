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
  }
}
