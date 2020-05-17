using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Scope.Views.Behaviors
{
  public class CommandOnClick:Behavior<Panel>
  {
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register("Command",
                                    typeof(ICommand),
                                    typeof(CommandOnClick));
    
    public ICommand Command
    {
      get { return (ICommand)GetValue(CommandProperty); }
      set { SetValue(CommandProperty, value); }
    }

    protected override void OnAttached()
    {
      base.OnAttached();

      AssociatedObject.MouseLeftButtonUp += CallCommand;
    }

    private void CallCommand(object sender, MouseButtonEventArgs e)
    {
      Command.Execute(null);
    }

    protected override void OnDetaching()
    {
      base.OnDetaching();
      AssociatedObject.MouseLeftButtonUp -= CallCommand;
    }
  }
}
