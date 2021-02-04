using Scope.Interfaces;
using System.IO;
using System.Linq;
using System.Text;

namespace Scope.FileViewer.DataForge.Models
{
  internal class File : IFile
  {
    private readonly byte[] _bytes;
    private readonly Struct _dataForgeItem;

    public File(string name, string path, Struct dataForgeItem)
    {
      Name = $"{name}.json";
      Path = path;
      _dataForgeItem = dataForgeItem;

      var json = ToJson(dataForgeItem);
      _bytes = Encoding.UTF8.GetBytes(json);

      BytesUncompressed = _bytes.Length;
      BytesCompressed = _bytes.Length;
    }

    private string ToJson(Struct dfi)
    {
      var b = new StringBuilder();
      b.Append("{\r\n");

      int i = 1;

      b.Append($"{Indent(i)}name: { dfi.Name}\r\n");

      foreach (var property in dfi.Properties)
      {
        b.Append(Serialize(property, i));
      }

      b.Append("}\r\n");

      return b.ToString();
    }

    private string Indent(int i)
    {
      return Enumerable.Repeat(" ", i)
                       .Aggregate((c, n) => $"{c}{n}");
    }

    private static string Serialize(Property property, int indent)
    {
      return $"{property.Name}: { property.Value}\r\n";
    }

    public int Index { get; }
    public string Name { get; }
    public string Path { get; }
    public long BytesCompressed { get; }
    public long BytesUncompressed { get; }

    public Stream Read()
    {
      return new MemoryStream(_bytes);
    }
  }
}
