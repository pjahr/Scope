using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Scope.Views.Behaviors
{
  public class AcquireKeyboardFocusOnClick : Behavior<Panel>
  {
    protected override void OnAttached()
    {
      base.OnAttached();

      AssociatedObject.MouseLeftButtonUp += AcquireKeyboardFocus;
    }

    private void AcquireKeyboardFocus(object _, MouseButtonEventArgs __)
    {
      Keyboard.Focus(AssociatedObject);
    }

    protected override void OnDetaching()
    {
      base.OnDetaching();
      AssociatedObject.MouseLeftButtonUp -= AcquireKeyboardFocus;
    }
  }
}
