using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Xaml.Behaviors;

namespace Scope.Views.Behaviors
{
  public class TextBoxCommitOnChange : Behavior<TextBox>
  {
    protected override void OnAttached()
    {
      AssociatedObject.TextChanged += RaiseChangeOnTextChange;
    }

    protected override void OnDetaching()
    {
      AssociatedObject.TextChanged -= RaiseChangeOnTextChange;
    }

    private void RaiseChangeOnTextChange(object _, TextChangedEventArgs e)
    {
      BindingOperations.GetBindingExpression(AssociatedObject, TextBox.TextProperty).UpdateSource();
    }
  }
}
