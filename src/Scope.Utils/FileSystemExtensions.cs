using Scope.Interfaces;
using System.IO;
using System.Linq;

namespace Scope.Utils
{
  public static class FileSystemExtensions
  {
    public static string GetFileName(this string path)
    {
      var beginOfFileName = path.LastIndexOf('/') + 1;
      return new string(path.Skip(beginOfFileName)
                            .ToArray());
    }

    public static string GetExtension(this IFile file)
    {
      return file.Path.GetExtension();
    }

    public static string GetExtension(this string path)
    {
      var i = path.LastIndexOf('.') + 1;
      if (i <= 0)
      {
        return "";
      }

      var extension = path.Substring(i, path.Length - i);
      return extension;
    }

    public static byte[] ReadAllBytes(this Stream s)
    {
      using (MemoryStream ms = new MemoryStream())
      {
        long oldPosition = s.Position;
        s.Position = 0;
        s.CopyTo(ms);
        s.Position = oldPosition;

        return ms.ToArray();
      }
    }
  }
}
