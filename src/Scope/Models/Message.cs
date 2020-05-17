using Scope.Models.Interfaces;

namespace Scope.Models
{
  internal class Message : IMessage
  {
    public Message(string text)
    {
      Text = text;
    }

    public string Text { get; }
  }
}
