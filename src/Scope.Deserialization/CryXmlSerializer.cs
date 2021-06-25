using System;
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
      return ReadStream(File.OpenRead(file), writeLog:true);
    }

    public static XmlDocument ReadStream(Stream stream,
                                         ByteOrderEnum byteOrder = ByteOrderEnum.LittleEndian,
                                         bool writeLog = false)
    {
      using (BinaryReader r = new BinaryReader(stream))
      {
        string header = r.ReadFString(8);

        if (header == "CryXml" || header == "CryXmlB")
        {
          //var cString = r.ReadCString();
        }
        else if (header == "CRY3SDK")
        {
          var bytes = r.ReadBytes(2);
        }
        else
        {
          throw new FormatException("Unknown File Format");
        }

        //var headerLength = r.BaseStream.Position;

        //byteOrder = ByteOrderEnum.BigEndian;

        var fileLength = r.ReadInt(byteOrder);

        //if (fileLength != r.BaseStream.Length)
        //{
        //  r.BaseStream.Seek(headerLength, SeekOrigin.Begin);
        //  byteOrder = ByteOrderEnum.LittleEndian;
        //  fileLength = r.ReadInt(byteOrder);
        //}

        var nodeTableOffset = r.ReadInt(byteOrder);
        var nodeTableCount = r.ReadInt(byteOrder);
        var nodeTableSize = 28;

        var attributeTableOffset = r.ReadInt(byteOrder);
        var attributeTableCount = r.ReadInt(byteOrder);
        //var referenceTableSize = 8;

        var childTableOffset = r.ReadInt(byteOrder);
        var childTableCount = r.ReadInt(byteOrder);
        //var length3 = 4;

        var stringTableOffset = r.ReadInt(byteOrder);
        var stringTableCount = r.ReadInt(byteOrder);

        if (writeLog)
        {
          // Regex byteFormatter = new Regex("([0-9A-F]{8})");
          //Console.WriteLine("Header");
          //Console.WriteLine("0x{0:X6}: {1}", 0x00, header);
          //Console.WriteLine("0x{0:X6}: {1:X8} (Dec: {1:D8})", headerLength + 0x00, fileLength);
          //Console.WriteLine("0x{0:X6}: {1:X8} (Dec: {1:D8}) node offset",
          //                  headerLength + 0x04,
          //                  nodeTableOffset);
          //Console.WriteLine("0x{0:X6}: {1:X8} (Dec: {1:D8}) nodes",
          //                  headerLength + 0x08,
          //                  nodeTableCount);
          //Console.WriteLine("0x{0:X6}: {1:X8} (Dec: {1:D8}) reference offset",
          //                  headerLength + 0x12,
          //                  attributeTableOffset);
          //Console.WriteLine("0x{0:X6}: {1:X8} (Dec: {1:D8}) references",
          //                  headerLength + 0x16,
          //                  attributeTableCount);
          //Console.WriteLine("0x{0:X6}: {1:X8} (Dec: {1:D8}) child offset",
          //                  headerLength + 0x20,
          //                  childTableOffset);
          //Console.WriteLine("0x{0:X6}: {1:X8} (Dec: {1:D8}) child",
          //                  headerLength + 0x24,
          //                  childTableCount);
          //Console.WriteLine("0x{0:X6}: {1:X8} (Dec: {1:D8}) content offset",
          //                  headerLength + 0x28,
          //                  stringTableOffset);
          //Console.WriteLine("0x{0:X6}: {1:X8} (Dec: {1:D8}) content",
          //                  headerLength + 0x32,
          //                  stringTableCount);
          //Console.WriteLine("");
          //Console.WriteLine("Node Table");
        }

        List<CryXmlNode> nodeTable = new List<CryXmlNode>();
        r.BaseStream.Seek(nodeTableOffset, SeekOrigin.Begin);
        int nodeID = 0;
        while (r.BaseStream.Position < nodeTableOffset + nodeTableCount * nodeTableSize)
        {
          var position = r.BaseStream.Position;
          var value = new CryXmlNode
          {
            NodeID = nodeID++,
            NodeNameOffset = r.ReadInt(byteOrder),
            ContentOffset = r.ReadInt(byteOrder),
            AttributeCount = r.ReadInt16(byteOrder),
            ChildCount = r.ReadInt16(byteOrder),
            ParentNodeID = r.ReadInt(byteOrder),
            FirstAttributeIndex = r.ReadInt(byteOrder),
            FirstChildIndex = r.ReadInt(byteOrder),
            Reserved = r.ReadInt(byteOrder)
          };

          nodeTable.Add(value);
          if (writeLog)
          {
            Console.WriteLine("0x{0:X6}: {1:X8} {2:X8} attr:{3:X4} {4:X4} {5:X8} {6:X8} {7:X8} {8:X8}",
                              position,
                              value.NodeNameOffset,
                              value.ContentOffset,
                              value.AttributeCount,
                              value.ChildCount,
                              value.ParentNodeID,
                              value.FirstAttributeIndex,
                              value.FirstChildIndex,
                              value.Reserved);
          }
        }

        if (writeLog)
        {
          Console.WriteLine("");
          Console.WriteLine("Reference Table");
        }

        List<CryXmlReference> attributeTable = new List<CryXmlReference>();
        r.BaseStream.Seek(attributeTableOffset, SeekOrigin.Begin);
        while (r.BaseStream.Position
               < attributeTableOffset + attributeTableCount * referenceTableSize)
        {
          var position = r.BaseStream.Position;
          var value = new CryXmlReference
          {
            NameOffset = r.ReadInt(byteOrder),
            ValueOffset = r.ReadInt(byteOrder)
          };

          attributeTable.Add(value);
          if (writeLog)
          {
            Console.WriteLine("0x{0:X6}: {1:X8} {2:X8}",
                              position,
                              value.NameOffset,
                              value.ValueOffset);
          }
        }

        if (writeLog)
        {
          Console.WriteLine("");
          Console.WriteLine("Order Table");
        }

        List<int> parentTable = new List<int>();
        r.BaseStream.Seek(childTableOffset, SeekOrigin.Begin);
        while (r.BaseStream.Position < childTableOffset + childTableCount * length3)
        {
          var position = r.BaseStream.Position;
          var value = r.ReadInt(byteOrder);

          parentTable.Add(value);
          if (writeLog)
          {
            Console.WriteLine("0x{0:X6}: {1:X8}", position, value);
          }
        }

        if (writeLog)
        {
          Console.WriteLine("");
          Console.WriteLine("Dynamic Dictionary");
        }

        List<CryXmlValue> dataTable = new List<CryXmlValue>();
        r.BaseStream.Seek(stringTableOffset, SeekOrigin.Begin);
        while (r.BaseStream.Position < r.BaseStream.Length)
        {
          var position = r.BaseStream.Position;
          var value = new CryXmlValue
          {
            Offset = (int)position - stringTableOffset,
            Value = r.ReadCString()
          };

          dataTable.Add(value);

          if (writeLog)
          {
            Console.WriteLine("0x{0:X6}: {1:X8} {2}", position, value.Offset, value.Value);
          }
        }

        var dataMap = dataTable.ToDictionary(k => k.Offset, v => v.Value);

        var attributeIndex = 0;

        var xmlDoc = new XmlDocument();

        Dictionary<int, XmlElement> xmlMap = new Dictionary<int, XmlElement>();

        foreach (var node in nodeTable)
        {
          XmlElement element = xmlDoc.CreateElement(dataMap[node.NodeNameOffset]);

          for (int i = 0,
                   j = node.AttributeCount;
               i < j;
               i++)
          {
            if (dataMap.ContainsKey(attributeTable[attributeIndex]
                                     .ValueOffset))
            {
              element.SetAttribute(dataMap[attributeTable[attributeIndex]
                                            .NameOffset],
                                   dataMap[attributeTable[attributeIndex]
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

          if (xmlMap.ContainsKey(node.ParentNodeID))
          {
            xmlMap[node.ParentNodeID]
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
