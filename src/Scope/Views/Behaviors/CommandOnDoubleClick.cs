using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Scope.Views.Behaviors
{
  public class CommandOnDoubleClick : Behavior<Panel>
  {
    public static readonly DependencyProperty CommandProperty =
      DependencyProperty.Register("Command", typeof(ICommand), typeof(CommandOnDoubleClick));

    public ICommand Command
    {
      get => (ICommand) GetValue(CommandProperty);
      set => SetValue(CommandProperty, value);
    }

    protected override void OnAttached()
    {
      base.OnAttached();

      AssociatedObject.MouseDown += CallCommandOnDoubleClick;
    }

    private void CallCommandOnDoubleClick(object sender, MouseButtonEventArgs e)
    {
      if (e.ClickCount != 2)
      {
        return;
      }

      Command.Execute(null);
    }

    protected override void OnDetaching()
    {
      base.OnDetaching();
      AssociatedObject.MouseLeftButtonUp -= CallCommandOnDoubleClick;
    }
  }
}
