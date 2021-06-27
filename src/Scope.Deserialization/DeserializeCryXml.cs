using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Scope.Deserialization
{
  public class DeserializeCryXml
  {
    private readonly CryXmlHeader _header;
    private readonly List<int> _parents = new List<int>();
    private readonly List<CryXmlValue> _values = new List<CryXmlValue>();
    private readonly List<CryXmlNode> _nodes = new List<CryXmlNode>();
    private readonly List<CryXmlAttribute> _attributes = new List<CryXmlAttribute>();

    public DeserializeCryXml(Stream stream, ByteOrderEnum byteOrder = ByteOrderEnum.BigEndian)
    {
      using (BinaryReader r = new BinaryReader(stream))
      {
        _header = new CryXmlHeader(r, byteOrder);

        ReadNodes(r);
        ReadAttributes(r);
        ReadNodeHierarchy(r);
        ReadValueStrings(r);

        Result = GenerateXml();
      }
    }

    private XmlDocument GenerateXml()
    {
      var dataMap = _values.ToDictionary(k => k.Offset, v => v.Value);
      var xmlDoc = new XmlDocument();
      var xmlMap = new Dictionary<int, XmlElement>();

      var attributeIndex = 0;
      foreach (var node in _nodes)
      {
        var element = xmlDoc.CreateElement(dataMap[node.ElementNameOffset]);

        for (int i = 0,
                 j = node.AttributeCount;
             i < j;
             i++)
        {
          if (dataMap.ContainsKey(_attributes[attributeIndex]
                                   .ValueOffset))
          {
            element.SetAttribute(dataMap[_attributes[attributeIndex]
                                          .NameOffset],
                                 dataMap[_attributes[attributeIndex]
                                          .ValueOffset]);
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

    private void ReadValueStrings(BinaryReader r)
    {
      r.BaseStream.Seek(_header.StringTableOffset, SeekOrigin.Begin);

      while (r.BaseStream.Position < r.BaseStream.Length)
      {
        _values.Add(new CryXmlValue(r, _header.StringTableOffset));
      }
    }

    private void ReadNodeHierarchy(BinaryReader r)
    {
      r.BaseStream.Seek(_header.ChildTableOffset, SeekOrigin.Begin);

      for (int i = 0; i < _header.ChildTableCount; i++)
      {
        _parents.Add(r.ReadInt());
      }
    }

    private void ReadAttributes(BinaryReader r)
    {
      r.BaseStream.Seek(_header.AttributeTableOffset, SeekOrigin.Begin);

      for (int i = 0; i < _header.AttributeTableCount; i++)
      {
        _attributes.Add(new CryXmlAttribute(r));
      }
    }

    private void ReadNodes(BinaryReader r)
    {
      r.BaseStream.Seek(_header.NodeTableOffset, SeekOrigin.Begin);

      for (int i = 0; i < _header.NodeTableCount; i++)
      {
        _nodes.Add(new CryXmlNode(r, i));
      }
    }

    public XmlDocument Result { get;}
  }
}
