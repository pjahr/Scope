using System;
using System.Collections.Generic;

namespace Scope.Models.Interfaces
{
  public interface IMessages
  {
    IReadOnlyCollection<IMessage> Items { get; }

    event Action MessageReceived;
  }
}
