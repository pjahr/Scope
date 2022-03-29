using System;

namespace Scope.Models.Interfaces
{
  public interface IDialog : IDisposable
  {
    event Action CloseRequested;
  }
}
