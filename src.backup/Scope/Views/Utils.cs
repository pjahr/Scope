using System.Windows;
using System.Windows.Media;

namespace Scope.Views
{
  internal static class Utils
  {
    public static FrameworkElement FindFocusableChild(this DependencyObject parent)
    {
      if (parent == null)
      {
        return null;
      }

      int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

      FrameworkElement found = null;
      for (int i = 0; i < childrenCount; i++)
      {
        var child = VisualTreeHelper.GetChild(parent, i);

        if (child is FrameworkElement && ((FrameworkElement) child).Focusable)
        {
          found = (FrameworkElement) child;
          break;
        }

        found = FindFocusableChild(child);
      }

      return found;
    }

    public static T FindChild<T>(this DependencyObject parent) where T : DependencyObject
    {
      for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
      {
        var child = VisualTreeHelper.GetChild(parent, i);
        switch (child)
        {
          case T t:
            return t;
          default:
            var found = FindChild<T>(child);
            if (found != null)
            {
              return found;
            }

            break;
        }
      }

      return null;
    }

    public static T FindParent<T>(this DependencyObject child) where T : DependencyObject
    {
      var parentObject = GetParent(child);

      switch (parentObject)
      {
        case null:
          return null; //we've reached the end of the tree

        case T parent:
          return parent;

        default:
          return FindParent<T>(parentObject);
      }
    }

    public static DependencyObject GetParent(this DependencyObject child)
    {
      switch (child)
      {
        case null:
          return null;
        case ContentElement contentElement:
          FrameworkContentElement fce = contentElement as FrameworkContentElement;
        {
          //handle content elements separately
          var parent = ContentOperations.GetParent(contentElement);
          return parent ?? fce?.Parent;
        }
        //also try searching for parent in framework elements (such as DockPanel, etc)
        case FrameworkElement frameworkElement:
        {
          var parent = frameworkElement.Parent;
          if (parent != null)
          {
            return parent;
          }

          break;
        }
      }

      //if it's not a ContentElement/FrameworkElement, rely on VisualTreeHelper
      return VisualTreeHelper.GetParent(child);
    }
  }
}
