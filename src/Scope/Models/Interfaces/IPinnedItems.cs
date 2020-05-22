using System;
using System.Collections.Generic;
using Scope.Interfaces;

namespace Scope.Models.Interfaces
{
  internal interface IPinnedItems
  {
    event Action Changed;

    IReadOnlyList<IDirectory> Directories { get; }
    IReadOnlyList<IFile> Files { get; }

    void Clear();
    void Add(params IDirectory[] directories);
    void Add(params IFile[] files);
    void Remove(params IDirectory[] directories);
    void Remove(params IFile[] files);
  }
}
