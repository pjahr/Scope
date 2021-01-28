using Scope.FileViewer.Text.Models;
using System.IO;
using System.Text;
using System.Xml;

namespace CryXmlConverter
{
  internal class Program
  {
    internal static void Main(string[] args)
    {
      // TODO sanitize input
      foreach (var path in args)
      {
        XmlDocument x;

        using (var s = File.OpenRead(path))
        {
          var header = new byte[4];
          s.Read(header, 0, 4);
          var headerText = Encoding.UTF8.GetString(header);
          x = CryXmlSerializer.ReadStream(s);
        }

        x.Save($"{path}.xml");
      }
    }
  }
}
