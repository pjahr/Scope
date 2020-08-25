using System;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace Scope.Models.Interfaces
{
  internal interface ICurrentP4k : IDisposable
  {
    IFileSystem FileSystem { get; }
    string FileName { get; }
    Task<OpenP4kFileResult> ChangeAsync(IFileInfo p4kFile);
    Task CloseAsync();
    event Action Changed;
    bool IsInitialized { get; }
  }
}
