using Scope.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Scope.FileViewer.DataForge.Models
{
  public class File : IFile
  {
    private readonly byte[] _bytes;

    public File(string name, string path, Struct dataForgeItem)
    {
      //if (name.Contains("AIShip_CrewProfiles-Human-OMC-Gunner-Gunner_Generic_01"))
      if (name.Contains("EntityClassDefinition.Actor"))        
      {
      }

      Name = $"{name}.json";
      Path = path;

      var b = new StringBuilder();
      int i = 0;
      Serialize(name, dataForgeItem, i, b);
      var json = b.ToString();
      _bytes = Encoding.UTF8.GetBytes(json);
      
      BytesUncompressed = _bytes.Length;
      BytesCompressed = _bytes.Length;
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

    private static void Serialize(string name, Struct s, int i, StringBuilder b)
    {
      b.Append($"{Indent(i)}\"{name}\": {{\r\n");

      // single properties first
      foreach (var property in s.Properties.Where(p => !p.IsList))
      {
        Serialize(property, i + 1, b);
      }

      // then all list properties
      foreach (var property in s.Properties.Where(p=>p.IsList))
      {
        Serialize(property, i + 1, b);
      }

      b.Append($"{Indent(i)}}}\r\n");
    }

    private static string Indent(int i)
    {
      return new string(' ', i);
    }

    private static void Serialize(Property p, int i, StringBuilder b)
    {
      switch (p.Type)
      {
        case DataType.Class:
        case DataType.StrongPointer:
          SerializeComplexProperty(p, i + 1, b);
          break;
        case DataType.Reference:
        case DataType.WeakPointer:
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
        case DataType.Boolean:
          SerializeElementaryProperty(p, i + 1, b);
          break;
        default:
          b.Append($"{Indent(i + 1)}{p.Name}: \"Unknown DataType\"\r\n");
          break;
      }
    }

    private static void SerializeComplexProperty(Property p, int i, StringBuilder b)
    {
      if (p.IsList)
      {
        var list = (List<Property>)p.Value;

        if (list.Count == 0)
        {
          b.Append($"{Indent(i)}\"{p.Name}\": []\r\n");
          return;
        }

        b.Append($"{Indent(i)}\"{p.Name}\": [\r\n");

        foreach (var item in list)
        {
          if (item.Value as string != null)
          {
            SerializeElementaryProperty(new Property() { Name = p.Name, Type = DataType.String, Value = "NULL" }, i, b);
          }
          else
          {
            Serialize(item.Name, (Struct)item.Value, i + 2, b);
          }

          b.Append($"{Indent(i)}}}\r\n");
        }

        b.Append($"{Indent(i)}]\r\n");
        return;
      }

      // property is a single value

      b.Append($"{Indent(i)}\"{p.Name}\": {{\r\n");

      if (p.Value as string == null)
      {
        Serialize(p.Name, (Struct)p.Value, i + 1, b);
      }
      else
      {
        SerializeElementaryProperty(new Property() { Name = p.Name, Type = DataType.String, Value = "NULL" }, i, b);
      }

      b.Append($"{Indent(i)}}}");
    }

    private static void SerializeElementaryProperty(Property p, int i, StringBuilder b)
    {
      if (p.IsList)
      {
        var list = (List<Property>)p.Value;
        if (list.Count == 0)
        {
          b.Append($"{Indent(i)}\"{p.Name}\": []\r\n");
          return;
        }

        b.Append($"{Indent(i)}\"{p.Name}\": [\r\n");

        foreach (var item in list)
        {
          SerializeElementaryProperty(item, i, b);
        }

        b.Append($"{Indent(i)}]\r\n");
      }
      else
      {
        b.Append($"{Indent(i)}\"{p.Name}\": \"{p.Value}\"\r\n");
      }
    }
  }
}
