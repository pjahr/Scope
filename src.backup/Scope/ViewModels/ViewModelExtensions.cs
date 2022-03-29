using System;
using System.ComponentModel;

namespace Scope.ViewModels
{
  public static class ViewModelExtensions
  {
    private static readonly string[] Suffixes = {" B", " KB", " MB", " GB", " TB", " PB"};

    public static string ToFileSize(this long number, int precision = 2)
    {
      var v = Convert.ToDouble(number);

      // unit's number of bytes
      const double unit = 1024;

      // suffix counter
      int i = 0;
      // as long as we're bigger than a unit, keep going
      while (v > unit)
      {
        v /= unit;
        i++;
      }

      // apply precision and current suffix
      return Math.Round(v, precision) + Suffixes[i];
    }

  }
}
