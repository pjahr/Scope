using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Scope.Views.Behaviors
{
  public class TextBoxCallCommandOnKey : Behavior<TextBox>
  {
    private Key _key;

    public static readonly DependencyProperty CommandProperty;
    public static readonly DependencyProperty CommandParameterProperty;
    public static readonly DependencyProperty KeyProperty;

    static TextBoxCallCommandOnKey()
    {
      CommandProperty = DependencyProperty.Register(nameof(Command),
                                                    typeof(ICommand),
                                                    typeof(TextBoxCallCommandOnKey),
                                                    new PropertyMetadata(default(ICommand)));

      CommandParameterProperty = DependencyProperty.Register(nameof(CommandParameter),
                                                             typeof(object),
                                                             typeof(TextBoxCallCommandOnKey),
                                                             new PropertyMetadata(default(object)));

      KeyProperty = DependencyProperty.Register(nameof(Key),
                                                typeof(string),
                                                typeof(TextBoxCallCommandOnKey),
                                                new PropertyMetadata(default(string),
                                                                     PropertyChangedCallback));
    }


    public object CommandParameter
    {
      get => GetValue(CommandParameterProperty);
      set => SetValue(CommandParameterProperty, value);
    }

    public ICommand Command
    {
      get => (ICommand) GetValue(CommandProperty);
      set => SetValue(CommandProperty, value);
    }

    public string Key
    {
      get => (string) GetValue(KeyProperty);
      set => SetValue(KeyProperty, value);
    }

    protected override void OnAttached()
    {
      if (_key == System.Windows.Input.Key.Escape) 
      {
        AssociatedObject.KeyUp += ExecuteCommandIfPossible;
      }
      else
      {
        AssociatedObject.PreviewKeyDown += ExecuteCommandIfPossible;
      }
    }

    protected override void OnDetaching()
    {
      AssociatedObject.PreviewKeyDown -= ExecuteCommandIfPossible;
      AssociatedObject.KeyUp -= ExecuteCommandIfPossible;
    }

    private static void PropertyChangedCallback(DependencyObject o,
                                                DependencyPropertyChangedEventArgs e)
    {
      var key = (string) e.NewValue;
      ((TextBoxCallCommandOnKey) o)._key = (Key) Enum.Parse(typeof(Key), key);
    }

    private void ExecuteCommandIfPossible(object sender, KeyEventArgs e)
    {
      if (e.Key != _key)
      {
        return;
      }

      if (Command == null)
      {
        return;
      }

      if (Command.CanExecute(CommandParameter))
      {
        Command.Execute(CommandParameter);
      }
    }
  }
}
