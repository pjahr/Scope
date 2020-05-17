using System;
using System.Collections.Generic;

namespace Scope.Models.Interfaces
{
  public interface IMessages
  {
    IReadOnlyCollection<IMessage> Items { get; }

    void Add(string text);

    event Action MessageReceived;
  }
}
