using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Scope.Views.Behaviors
{
  public class CommandOnKey : Behavior<FrameworkElement>
  {
    public static readonly DependencyProperty KeyProperty = DependencyProperty.Register("Key", typeof(Key), typeof(CommandOnKey));
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(CommandOnKey));
    public static readonly DependencyProperty CommandParameterProperty =
      DependencyProperty.Register("CommandParameter", typeof(object), typeof(CommandOnKey));

    public Key Key { get => (Key) GetValue(KeyProperty); set => SetValue(KeyProperty, value); }
    public ICommand Command { get => (ICommand) GetValue(CommandProperty); set => SetValue(CommandProperty, value); }
    public object CommandParameter { get => GetValue(CommandParameterProperty); set => SetValue(CommandParameterProperty, value); }

    protected override void OnAttached()
    {
      base.OnAttached();

      AssociatedObject.PreviewKeyDown += CallCommandOnKey;
    }

    private void CallCommandOnKey(object sender, KeyEventArgs e)
    {
      if (e.Key != Key)
      {
        return;
      }

      Command.Execute(CommandParameter);
      e.Handled = true;
    }

    protected override void OnDetaching()
    {
      base.OnDetaching();
      AssociatedObject.KeyDown -= CallCommandOnKey;
    }
  }
}
