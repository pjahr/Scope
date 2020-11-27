using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Scope.FileViewer.DataForge.Models
{
  public class StructDefinition
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

      if (name== "MeleeAttackInfo")
      {

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
            var structDefinition = df.StructDefinitionTable[propertyDefinition.StructIndex];
            var structData = structDefinition.Read(r, propertyDefinition.Name, df);
            properties.Add(new Property() { Name = structDefinition.Name, Type = DataType.Class, Value = structData });
          }
          else if (propertyDefinition.DataType == DataType.StrongPointer)
          {
            var structIndex = (ushort)r.ReadUInt32();
            var recordIndex = (int)r.ReadUInt32();

            var property = new Property() { Name = propertyDefinition.Name, Type = DataType.StrongPointer, Value = "some reference" };
            properties.Add(property);

            df.ClassMappings.Add(new ClassMapping
            {
              StructIndex = structIndex,
              RecordIndex = recordIndex,
              Property = property
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

          var elements = new List<Property>();
          var propName = propertyDefinition.Name;
          var propType = propertyDefinition.DataType;

          for (var i = 0; i < arrayCount; i++)
          {
            switch (propertyDefinition.DataType)
            {
              case DataType.Boolean:
                
                elements.Add(new Property { Name = propName, Type = propType, Value = df.BooleanValues[firstIndex + i]});
                break;

              case DataType.Double:
                elements.Add(new Property { Name = propName, Type = propType, Value = df.DoubleValues[firstIndex + i]});

                break;

              case DataType.Enum: //TODO uint, value is retrieved from ValueMap
                elements.Add(new Property { Name = propName, Type = propType, Value = df.EnumValues[firstIndex + i]});

                break;

              case DataType.Guid:
                elements.Add(new Property { Name = propName, Type = propType, Value = df.GuidValues[firstIndex + i]});

                break;

              case DataType.Int16:
                elements.Add(new Property { Name = propName, Type = propType, Value = df.Int16Values[firstIndex + i]});

                break;

              case DataType.Int32:
                elements.Add(new Property { Name = propName, Type = propType, Value = df.Int32Values[firstIndex + i]});

                break;

              case DataType.Int64:
                elements.Add(new Property{Name = propName,Type = propType,Value =df.Int64Values[firstIndex + i]});

                break;

              case DataType.SByte:
                elements.Add(new Property{Name = propName,Type = propType,Value =df.Int8Values[firstIndex + i]});

                break;

              case DataType.Locale:
                elements.Add(new Property{Name = propName,Type = propType,Value =df.LocaleValues[firstIndex + i]});

                break;

              case DataType.Reference:
                elements.Add(new Property{Name = propName,Type = propType,Value =df.ReferenceValues[firstIndex + i]});

                break;

              case DataType.Single:
                elements.Add(new Property{Name = propName,Type = propType,Value =df.SingleValues[firstIndex + i]});

                break;

              case DataType.String:
                elements.Add(new Property{Name = propName,Type = propType,Value =df.StringValues[firstIndex + i]});

                break;

              case DataType.UInt16:
                elements.Add(new Property{Name = propName,Type = propType,Value =df.UInt16Values[firstIndex + i]});

                break;

              case DataType.UInt32:
                elements.Add(new Property{Name = propName,Type = propType,Value =df.UInt32Values[firstIndex + i]});

                break;

              case DataType.UInt64:
                elements.Add(new Property{Name = propName,Type = propType,Value =df.UInt64Values[firstIndex + i]});

                break;

              case DataType.Byte:
                elements.Add(new Property{Name = propName,Type = propType,Value =df.UInt8Values[firstIndex + i]});

                break;

              case DataType.Class:
                var property = new Property { Name = propName, Type = propType, Value = "MAPPING PLACEHOLDER" };
                elements.Add(property);
                df.ClassMappings.Add(new ClassMapping
                {
                  StructIndex = propertyDefinition.StructIndex,
                  RecordIndex = (int)(firstIndex + i),
                  Property = property
                });
                break;

              case DataType.StrongPointer:
                var property1 = new Property { Name = propName, Type = propType, Value = "MAPPING PLACEHOLDER" };
                elements.Add(property1);
                df.StrongMappings.Add(new ClassMapping
                {
                  StructIndex = propertyDefinition.StructIndex,
                  RecordIndex = (int)(firstIndex + i),
                  Property = property1
                });
                break;

              case DataType.WeakPointer:
                var property2 = new Property { Name = propName, Type = propType, Value = "MAPPING PLACEHOLDER" };
                elements.Add(property2);
                df.WeakMappings1.Add(new ClassMapping
                {
                  StructIndex = propertyDefinition.StructIndex,
                  RecordIndex = (int)(firstIndex + i),
                  Property = property2
                });
                break;

              default:
                throw new NotImplementedException();
            }
          }

          var listProperty = new Property
          {
            Name = propertyDefinition.Name,
            Type = propertyDefinition.DataType,
            IsList = true,
            Value = elements
          };

          properties.Add(listProperty);
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
