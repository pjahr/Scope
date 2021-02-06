using Scope.Interfaces;
using System.Collections.Generic;
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
        Serialize(property, i, b);
      }

      b.Append("}\r\n");

      return b.ToString();
    }

    private static string Indent(int i)
    {
      return Enumerable.Repeat(" ", i)
                       .Aggregate((c, n) => $"{c}{n}");
    }

    private static void Serialize(Property p, int i, StringBuilder b)
        {
            i++;
            switch (p.Type)
            {
                case DataType.Class:
                    b.Append($"{Indent(i)}{p.Name}: \"TODO: Class\"\r\n");
                    break;
                case DataType.Reference:
                case DataType.WeakPointer:
                case DataType.StrongPointer:
                case DataType.Enum:
                case DataType.Guid:
                case DataType.Locale:
                case DataType.Double:
                case DataType.Single:
                case DataType.String:
                case DataType.UInt64:
                case DataType.UInt32:
                case DataType.UInt16:
                case DataType.Byte:
                case DataType.Int64:
                case DataType.Int32:
                case DataType.Int16:
                case DataType.SByte:
                    SerializeSingleValueProperty(p, i + 1, b);
                    break;
                case DataType.Boolean:
                    b.Append( $"{Indent(i)}{p.Name}: {p.Value}\r\n");
                    break;
                default:
                    b.Append($"{Indent(i)}{p.Name}: \"Unknown DataType\"\r\n");
                    break;
            }
        }

        private static void SerializeSingleValueProperty(Property p, int i, StringBuilder b)
        {
            if(p.IsList)
            {
                b.Append($"{Indent(i)}[\r\n");

                foreach (var item in (List<Property>)p.Value)
                {
                    SerializeSingleValueProperty(item, i + 1, b);
                }

                b.Append($"{Indent(i)}]\r\n");
            }
            b.Append($"{Indent(i)}{p.Name}: \"{ p.Value}\"\r\n");
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
