using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Scope.Views.Behaviors
{
  public class CommandOnReceiveKeyboardFocus : Behavior<Panel>
  {
    public static readonly DependencyProperty CommandProperty =
      DependencyProperty.Register("Command", typeof(ICommand), typeof(CommandOnReceiveKeyboardFocus));

    public ICommand Command { get => (ICommand) GetValue(CommandProperty); set => SetValue(CommandProperty, value); }

    protected override void OnAttached()
    {
      base.OnAttached();

      AssociatedObject.GotKeyboardFocus += CallCommand;
    }

    private void CallCommand(object sender, KeyboardFocusChangedEventArgs e)
    {
      Command.Execute(null);
    }

    protected override void OnDetaching()
    {
      base.OnDetaching();
      AssociatedObject.GotKeyboardFocus -= CallCommand;
    }
  }
}
