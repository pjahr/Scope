using System;

namespace Scope.Interfaces
{
  public static class ExternalResourceDictionaryUri
  {
    public static Uri Create<TResourceDictionary>()
    {
      var type = typeof(TResourceDictionary);

      return new Uri($"{type.Assembly.GetName()};component/{type.Name}.xaml",
                     UriKind.Relative);
    }
  }
}
