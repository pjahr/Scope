using Scope.Models.Interfaces;

namespace Scope.Models
{
  internal class P4kFileSystem : IFileSystem
  {
    public P4kFileSystem(IDirectory root)
    {
      Root = root;
    }

    public IDirectory Root { get; }
  }
}
