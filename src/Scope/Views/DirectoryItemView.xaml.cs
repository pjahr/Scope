using System.Windows.Controls;
using System.Windows.Input;

namespace Scope.Views
{
  public partial class DirectoryItemView : UserControl
  {
    public DirectoryItemView()
    {
      InitializeComponent();
    }

    private void NavigateIfNecessary(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Up)
      {
        var listBox = this.FindParent<ListBox>();
        var item = this.FindParent<ListBoxItem>();

        var i = listBox.Items.IndexOf(item.DataContext);

        if (i == 0)
        {
          // focus parent
          var parentItem = listBox.FindParent<ListBoxItem>();
          Keyboard.Focus(parentItem.FindFocusableChild());
          e.Handled = true;
          return;
        }

        var listBoxItem =
          (ListBoxItem) listBox.ItemContainerGenerator.ContainerFromItem(listBox.Items[i - 1]);
        Keyboard.Focus(listBoxItem.FindFocusableChild());
        e.Handled = true;
        return;
      }

      if (e.Key == Key.Down)
      {
        var listBox = this.FindParent<ListBox>();
        var item = this.FindParent<ListBoxItem>();

        var i = listBox.Items.IndexOf(item.DataContext);

        if (i == listBox.Items.Count - 1)
        {
          // focus parents successor
          //var parentBox = listBox.TryFindParent<ListBox>();
          //var parentItem = listBox.TryFindParent<ListBoxItem>();
          //parentItem.Focus();
          e.Handled = true;
          return;
        }

        var listBoxItem =
          (ListBoxItem) listBox.ItemContainerGenerator.ContainerFromItem(listBox.Items[i + 1]);
        Keyboard.Focus(listBoxItem.FindFocusableChild());
        e.Handled = true;
      }
    }
  }
}
