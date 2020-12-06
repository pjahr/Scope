using Scope.Interfaces;

namespace Scope.ViewModels.Factories
{
  internal interface IFileSystemTreeNodeViewModelFactory
  {
    FileTreeNodeViewModel Create(IFile file);
    DirectoryTreeNodeViewModel Create(IDirectory directory);
  }
}
