using System;

namespace Scope.Interfaces
{
  public interface IFileViewer : IDisposable
  {
    string Header { get; }
  }
}
