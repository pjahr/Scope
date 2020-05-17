using Scope.Models.Interfaces;

namespace Scope.Models
{
    internal class P4kFileSystemFactory : IP4kFileSystemFactory
    {
        public IFileSystem Create(IDirectory root)
        {
            return new P4kFileSystem(root);
        }
    }
}
