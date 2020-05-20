using Scope.Interfaces;
using Scope.Models.Interfaces;

namespace Scope.Models
{
  internal class ExtractP4kContent : IExtractP4kContent
  {
    private readonly ICurrentP4k _currentP4K;

    public ExtractP4kContent(ICurrentP4k currentP4k)
    {
      _currentP4K = currentP4k;
    }

    public void Extract(IFile file)
    {
      
    }

    public void Extract(IDirectory directory)
    {
      
    }
  }
}
