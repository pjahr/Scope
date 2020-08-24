using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Scope.FileViewer.DataForge.Models
{
  internal class StructDefinition
  {
    private readonly Func<uint, string> _valueOf;

    public StructDefinition(BinaryReader r, Func<uint, string> valueOf)
    {
      _valueOf = valueOf;

      NameOffset = r.ReadUInt32();
      ParentTypeIndex = r.ReadUInt32();
      AttributeCount = r.ReadUInt16();
      FirstAttributeIndex = r.ReadUInt16();
      NodeType = r.ReadUInt32();
    }

    public uint NameOffset { get; set; }
    public uint ParentTypeIndex { get; set; }
    public ushort AttributeCount { get; set; }
    public ushort FirstAttributeIndex { get; set; }
    public uint NodeType { get; set; }

    public PropertyDefinition[] Properties { get; private set; }

    public string Name => _valueOf(NameOffset);
    //public String __parentTypeIndex { get { return String.Format("{0:X4}", ParentTypeIndex); } }
    //public String __attributeCount { get { return String.Format("{0:X4}", AttributeCount); } }
    //public String __firstAttributeIndex { get { return String.Format("{0:X4}", FirstAttributeIndex); } }
    //public String __nodeType { get { return String.Format("{0:X4}", NodeType); } }

    public void Read(BinaryReader r, string name, DataForgeFile df)
    {
      var baseStruct = this;
      var properties = new List<PropertyDefinition>();

     
      // TODO: Do we need to handle property overrides (original comment, investigate)

      // TODO: Include 1st call in while loop (same call inside)?

      var propertyDefinitions = Enumerable.Range(FirstAttributeIndex, AttributeCount)
                                          .Select(i => df.PropertyDefinitionTable[i])
                                          .ToArray();

      properties.InsertRange(0, propertyDefinitions);

      while (baseStruct.ParentTypeIndex != 0xFFFFFFFF)
      {
        baseStruct = df.StructDefinitionTable[Convert.ToInt32(baseStruct.ParentTypeIndex)];

        var attributes = Enumerable.Range(baseStruct.FirstAttributeIndex, baseStruct.AttributeCount)
                                   .Select(i => df.PropertyDefinitionTable[i])
                                   .ToArray();
        properties.InsertRange(0,
                               attributes);
      }

      foreach (var propertyDefinition in properties)
      {
        propertyDefinition.ConversionType =
          (ConversionType)((int)propertyDefinition.ConversionType & 0xFF);

        if (propertyDefinition.ConversionType == ConversionType.Attribute)
        {
          // ConversionType seems only used to differentiate between arrays and single values

          if (propertyDefinition.DataType == DataType.Class)
          {
            var dataStruct = df.StructDefinitionTable[propertyDefinition.StructIndex];
            dataStruct.Read(r, propertyDefinition.Name, df);
          }
          else if (propertyDefinition.DataType == DataType.StrongPointer)
          {
            var structIndex = (ushort)r.ReadUInt32();
            var recordIndex = (int)r.ReadUInt32();

            df.ClassMappings.Add(new ClassMapping
            {
              StructIndex = structIndex,
              RecordIndex = recordIndex
            });
          }
          else
          {
            propertyDefinition.Read(r, df);
          }
        }
        else
        {
          // ConversionType seems only used to differentiate between arrays and single values
          var arrayCount = r.ReadUInt32();
          var firstIndex = r.ReadUInt32();
          
          var elements = new List<object>();

          for (var i = 0; i < arrayCount; i++)
          {
            switch (propertyDefinition.DataType)
            {
              case DataType.Boolean:
                elements.Add(df.BooleanValues[firstIndex + i]);
                break;

              case DataType.Double:
                elements.Add(df.DoubleValues[firstIndex + i]);

                break;

              case DataType.Enum: //TODO uint, value is retrieved from ValueMap
                elements.Add(df.EnumValues[firstIndex + i]);

                break;

              case DataType.Guid:
                elements.Add(df.GuidValues[firstIndex + i]);

                break;

              case DataType.Int16:
                elements.Add(df.Int16Values[firstIndex + i]);

                break;

              case DataType.Int32:
                elements.Add(df.Int32Values[firstIndex + i]);

                break;

              case DataType.Int64:
                elements.Add(df.Int64Values[firstIndex + i]);

                break;

              case DataType.SByte:
                elements.Add(df.Int8Values[firstIndex + i]);

                break;

              case DataType.Locale:
                elements.Add(df.LocaleValues[firstIndex + i]);

                break;

              case DataType.Reference:
                elements.Add(df.ReferenceValues[firstIndex + i]);

                break;

              case DataType.Single:
                elements.Add(df.SingleValues[firstIndex + i]);

                break;

              case DataType.String:
                elements.Add(df.StringValues[firstIndex + i]);

                break;

              case DataType.UInt16:
                elements.Add(df.UInt16Values[firstIndex + i]);

                break;

              case DataType.UInt32:
                elements.Add(df.UInt32Values[firstIndex + i]);

                break;

              case DataType.UInt64:
                elements.Add(df.UInt64Values[firstIndex + i]);

                break;

              case DataType.Byte:
                elements.Add(df.UInt8Values[firstIndex + i]);

                break;

              case DataType.Class:
                df.ClassMappings.Add(new ClassMapping
                {
                  StructIndex = propertyDefinition.StructIndex,
                  RecordIndex = (int)(firstIndex + i)
                });
                break;

              case DataType.StrongPointer:

                df.StrongMappings.Add(new ClassMapping
                {
                  StructIndex = propertyDefinition.StructIndex,
                  RecordIndex = (int)(firstIndex + i)
                });
                break;

              case DataType.WeakPointer:

                df.WeakMappings1.Add(new ClassMapping
                {
                  StructIndex = propertyDefinition.StructIndex,
                  RecordIndex = (int)(firstIndex + i)
                });
                break;

              default:
                throw new NotImplementedException();

                // var tempe = this.DocumentRoot.CreateElement(String.Format("{0}", node.DataType));
                // var tempa = this.DocumentRoot.CreateAttribute("__child");
                // tempa.Value = (firstIndex + i).ToString();
                // tempe.Attributes.Append(tempa);
                // var tempb = this.DocumentRoot.CreateAttribute("__parent");
                // tempb.Value = node.StructIndex.ToString();
                // tempe.Attributes.Append(tempb);
                // child.AppendChild(tempe);
                // break;
            }
          }
        }
      }

      Properties = properties.ToArray();
    }

    public override string ToString()
    {
      return $"{Name} ({AttributeCount}, {FirstAttributeIndex} ,{NodeType}, {ParentTypeIndex})";
    }
  }
}
