using System;
using System.Collections.Generic;

namespace Scope.Models.Interfaces
{
  //TODO: convert to RX subject.
  public interface IMessages
  {
    IReadOnlyCollection<IMessage> Items { get; }

    event Action MessageReceived;
  }
}
