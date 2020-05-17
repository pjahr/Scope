namespace Scope.Models.Interfaces
{
    internal interface IP4kFileSystemFactory
    {
        IFileSystem Create(IDirectory root);
    }
}
