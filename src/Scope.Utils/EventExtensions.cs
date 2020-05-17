using System;
using System.ComponentModel;

namespace Scope.Utils
{
  public static class EventExtensions
  {
    public static void Raise(this Action action)
    {
      action?.Invoke();
    }

    public static void Raise<T>(this Action<T> action, T p0)
    {
      action?.Invoke(p0);
    }

    public static void Raise(this EventHandler action, object sender)
    {
      action?.Invoke(sender, new EventArgs());
    }

    public static void Raise(this PropertyChangedEventHandler action, object sender, string propertyName)
    {
      action?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
    }
  }
}
