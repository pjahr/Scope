﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Scope.Models.Interfaces;
using Scope.Utils;

namespace Scope.Models
{
  [Export]
  internal class Messages : IMessages
  {
    Queue<IMessage> _messages = new Queue<IMessage>();

    public IReadOnlyCollection<IMessage> Items => _messages;

    public event Action MessageReceived;

    public void Add(string message)
    {
      _messages.Enqueue(new Message(message));

      MessageReceived.Raise();
    }
  }
}
