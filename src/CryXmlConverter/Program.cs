using Scope.FileViewer.Text.Models;
using System;
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
          var first10Bytes = new byte[10];
          s.Read(first10Bytes, 0, 10);
          var headerText = Encoding.UTF8.GetString(first10Bytes);
                    
          if (headerText.StartsWith("CryXmlB"))
          {
            x = CryXmlSerializer.ReadStream(s);
            x.Save($"{path}.xml");
          }

          if (headerText.StartsWith("CrChF"))
          {
            var contents = DeserializeChCr.Deserialize(s);

            for (int i = 0; i < contents.Length; i++)
            {
              File.WriteAllText($"{path}{i}.txt", contents[i]);
            }            
          }
        }
      }
    }
  }
}
