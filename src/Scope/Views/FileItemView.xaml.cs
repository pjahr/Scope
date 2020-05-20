using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Scope.Views
{
  public partial class FileItemView : UserControl
  {
    public FileItemView()
    {
      InitializeComponent();
    }

    private void NavigateIfNecessary(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Up)
      {
        NavigateUp();
        e.Handled = true;
        return;
      }

      if (e.Key == Key.Down)
      {
        NavigateDown();
        e.Handled = true;
      }
    }

    private void NavigateDown()
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
        return;
      }

      var listBoxItem =
        (ListBoxItem) listBox.ItemContainerGenerator.ContainerFromItem(listBox.Items[i + 1]);
      Keyboard.Focus(listBoxItem.FindFocusableChild());
    }

    private void NavigateUp()
    {
      var listBox = this.FindParent<ListBox>();
      var item = this.FindParent<ListBoxItem>();

      var i = listBox.Items.IndexOf(item.DataContext);

      if (i == 0)
      {
        // focus parent
        var parentItem = listBox.FindParent<ListBoxItem>();
        Keyboard.Focus(parentItem.FindFocusableChild());
        return;
      }

      var listBoxItem =
        (ListBoxItem) listBox.ItemContainerGenerator.ContainerFromItem(listBox.Items[i - 1]);
      var lastDeepestChild = GetLastDeepestChild(listBoxItem);
      Keyboard.Focus(lastDeepestChild.FindFocusableChild());
    }

    private ListBoxItem GetLastDeepestChild(ListBoxItem listBoxItem)
    {
      ListBox box = listBoxItem.FindChild<ListBox>();
      if (box == null)
      {
        return listBoxItem;
      }

      if (box.Items.Count == 0)
      {
        Console.WriteLine("Directory Listbox with 0 items!");
        return listBoxItem;
      }

      var lastItem =
        (ListBoxItem) box.ItemContainerGenerator.ContainerFromItem(box.Items[box.Items.Count - 1]);
      return GetLastDeepestChild(lastItem);
    }
  }
}
