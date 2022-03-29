using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Scope.Views.Converters
{
  public class EnumMatchToBooleanConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null || parameter == null)
      {
        return false;
      }

      var checkValue = value.ToString();
      var targetValue = parameter.ToString();
      return checkValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null || parameter == null)
      {
        return null;
      }

      var useValue = (bool)value;
      var targetValue = parameter.ToString();
      if (useValue)
      {
        return Enum.Parse(targetType, targetValue);
      }

      return null;
    }
  }
}
