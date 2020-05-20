using System.Windows.Input;

namespace Scope.ViewModels
{
  public abstract class MenuItemBase
  {
    public string Label { get; protected set; }
    public string Shortcut { get; protected set; }
    public ICommand Command { get; protected set; }
  }
}
