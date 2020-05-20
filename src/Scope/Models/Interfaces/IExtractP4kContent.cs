using Scope.Interfaces;

namespace Scope.Models.Interfaces
{
  internal interface IExtractP4kContent
  {
    void Extract(IFile file);
    void Extract(IDirectory directory);
  }
}
