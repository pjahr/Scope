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

    public string Name => _valueOf(NameOffset);
    
    public Struct Read(BinaryReader r, string name, DataForgeFile df)
    {
      var baseStruct = this;
      var propertyDefinitions = new List<PropertyDefinition>();


      // TODO: Do we need to handle property overrides (original comment, investigate)

      // TODO: Include 1st call in while loop (same call inside)?


      propertyDefinitions.InsertRange(0, Enumerable.Range(FirstAttributeIndex, AttributeCount)
                                          .Select(i => df.PropertyDefinitionTable[i])
                                          .ToArray());

      while (baseStruct.ParentTypeIndex != 0xFFFFFFFF)
      {
        baseStruct = df.StructDefinitionTable[Convert.ToInt32(baseStruct.ParentTypeIndex)];

        var attributes = Enumerable.Range(baseStruct.FirstAttributeIndex, baseStruct.AttributeCount)
                                   .Select(i => df.PropertyDefinitionTable[i])
                                   .ToArray();
        propertyDefinitions.InsertRange(0,
                               attributes);
      }
      
      var properties = new List<Property>();

      foreach (var propertyDefinition in propertyDefinitions)
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
            properties.Add(propertyDefinition.Read(r, df));
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

      return new Struct
      {
        Name = Name,
        Properties = properties.ToArray()
      };
    }

    public override string ToString()
    {
      return $"{Name} ({AttributeCount}, {FirstAttributeIndex}, {NodeType}, {ParentTypeIndex})";
    }
  }
}
