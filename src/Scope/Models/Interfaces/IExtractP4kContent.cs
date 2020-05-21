using Scope.Interfaces;

namespace Scope.Models.Interfaces
{
  internal interface IExtractP4kContent
  {
    void Extract(IFile file, string outputDirectoryPath);
    void Extract(IDirectory directory, string outputDirectoryPath);
  }
}
