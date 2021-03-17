using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using static System.Windows.Visibility;

namespace Scope.Views.Converters
{
  public class VisibleOrHidden : BoolToVisibilityConverter
  {
    public VisibleOrHidden() : base(Visible, Hidden) { }
  }

  public class HiddenOrVisible : BoolToVisibilityConverter
  {
    public HiddenOrVisible() : base(Hidden, Visible) { }
  }

  public abstract class BoolToVisibilityConverter : IValueConverter
  {
    private readonly Visibility _whenTrue;
    private readonly Visibility _whenFalse;

    protected BoolToVisibilityConverter(Visibility whenTrue, Visibility whenFalse)
    {
      _whenTrue = whenTrue;
      _whenFalse = whenFalse;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return ((bool)value) ? _whenTrue : _whenFalse;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
