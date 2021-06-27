using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Scope.Deserialization
{
  public static class CryXmlSerializer
  {
    public static XmlDocument ReadFile(string file)
    {
      return ReadStream(File.OpenRead(file));
    }

    public static XmlDocument ReadStream(Stream stream)
    {
      using (BinaryReader r = new BinaryReader(stream))
      {
        var header = new CryXmlHeader(r);

        r.BaseStream.Seek(header.NodeTableOffset, SeekOrigin.Begin);

        var nodes = new List<CryXmlNode>();
        for (int i = 0; i < header.NodeTableCount; i++)
        {
          nodes.Add(new CryXmlNode(r, i));
        }

        r.BaseStream.Seek(header.AttributeTableOffset, SeekOrigin.Begin);

        var attributes = new List<CryXmlAttribute>();

        for (int i = 0; i < header.AttributeTableCount; i++)
        {
          attributes.Add(new CryXmlAttribute(r));
        }

        r.BaseStream.Seek(header.ChildTableOffset, SeekOrigin.Begin);

        var parents = new List<int>();
        for (int i = 0; i < header.ChildTableCount; i++)
        {
          parents.Add(r.ReadInt());
        }

        r.BaseStream.Seek(header.StringTableOffset, SeekOrigin.Begin);

        //var dataTable = new List<CryXmlValue>();

        //for (int i = 0; i < header.StringTableSize; i++)
        //{
        //  dataTable.Add(new CryXmlValue(r, header.StringTableOffset));
        //}

        List<CryXmlValue> dataTable = new List<CryXmlValue>();
        r.BaseStream.Seek(header.StringTableOffset, SeekOrigin.Begin);
        while (r.BaseStream.Position < r.BaseStream.Length)
        {
          var position = r.BaseStream.Position;
          dataTable.Add(new CryXmlValue(r, header.StringTableOffset));
        }

        var dataMap = dataTable.ToDictionary(k => k.Offset, v => v.Value);

        var attributeIndex = 0;

        var xmlDoc = new XmlDocument();

        Dictionary<int, XmlElement> xmlMap = new Dictionary<int, XmlElement>();

        foreach (var node in nodes)
        {
          XmlElement element = xmlDoc.CreateElement(dataMap[node.ElementNameOffset]);

          for (int i = 0,
                   j = node.AttributeCount;
               i < j;
               i++)
          {
            if (dataMap.ContainsKey(attributes[attributeIndex]
                                     .ValueOffset))
            {
              element.SetAttribute(dataMap[attributes[attributeIndex]
                                            .NameOffset],
                                   dataMap[attributes[attributeIndex]
                                            .ValueOffset]);
            }
            else
            {
              throw new InvalidDataException("This is a bug according to the original code.");
              //element.SetAttribute(dataMap[attributeTable[attributeIndex].NameOffset], "BUGGED");
            }

            attributeIndex++;
          }

          xmlMap[node.NodeID] = element;

          if (dataMap.ContainsKey(node.ContentOffset))
          {
            if (!string.IsNullOrWhiteSpace(dataMap[node.ContentOffset]))
            {
              element.AppendChild(xmlDoc.CreateCDataSection(dataMap[node.ContentOffset]));
            }
          }
          else
          {
            throw new InvalidDataException("This is a bug according to the original code.");
            //element.AppendChild(xmlDoc.CreateCDataSection("BUGGED"));
          }

          if (xmlMap.ContainsKey(node.ParentNodeIndex))
          {
            xmlMap[node.ParentNodeIndex]
             .AppendChild(element);
          }
          else
          {
            xmlDoc.AppendChild(element);
          }
        }

        return xmlDoc;
      }
    }
  }
}
