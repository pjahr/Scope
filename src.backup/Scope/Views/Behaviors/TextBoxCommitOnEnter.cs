using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Scope.Views.Behaviors
{
  public class TextBoxCommitOnEnter : Behavior<TextBox>
  {
    protected override void OnAttached()
    {
      AssociatedObject.KeyDown += RaiseChangeOnEnterKey;
    }

    protected override void OnDetaching()
    {
      AssociatedObject.KeyDown -= RaiseChangeOnEnterKey;
    }

    private void RaiseChangeOnEnterKey(object _, KeyEventArgs e)
    {
      if (e.Key != Key.Enter)
      {
        return;
      }

      BindingOperations.GetBindingExpression(AssociatedObject, TextBox.TextProperty).UpdateSource();
    }
  }
}
