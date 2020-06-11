using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

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

    public PropertyDefinition[] Properties { get; }

    public string Name => _valueOf(NameOffset);
    //public String __parentTypeIndex { get { return String.Format("{0:X4}", ParentTypeIndex); } }
    //public String __attributeCount { get { return String.Format("{0:X4}", AttributeCount); } }
    //public String __firstAttributeIndex { get { return String.Format("{0:X4}", FirstAttributeIndex); } }
    //public String __nodeType { get { return String.Format("{0:X4}", NodeType); } }

    public void Read(BinaryReader r,
                     string name,
                     Func<int, StructDefinition> getStructDefinition,
                     Func<int, PropertyDefinition> getPropertyDefinition)
    {
      var baseStruct = this;
      var properties = new List<PropertyDefinition>();

      // TODO: Do we need to handle property overrides

      properties.AddRange(Enumerable.Range(FirstAttributeIndex, AttributeCount)
                                    .Select(getPropertyDefinition));

      while (baseStruct.ParentTypeIndex != 0xFFFFFFFF)
      {
        baseStruct = getStructDefinition(Convert.ToInt32(baseStruct.ParentTypeIndex));

        properties.AddRange(Enumerable.Range(FirstAttributeIndex, AttributeCount)
                                      .Select(getPropertyDefinition));
      }

      foreach (var node in properties)
      {
        node.ConversionType = (ConversionType) ((int) node.ConversionType & 0xFF);

        if (node.ConversionType == ConversionType.Attribute)
        {
          // ConversionType seems only used to differentiate between arrays and single values
          
          if (node.DataType == DataType.Class)
          {
            var dataStruct = getStructDefinition(node.StructIndex);
            dataStruct.Read(r, node.Name, getStructDefinition, getPropertyDefinition);
          }
          else if (node.DataType == DataType.StrongPointer)
          {
                        //var parentSP = this.DocumentRoot.CreateElement(node.Name);
                        //var emptySP = this.DocumentRoot.CreateElement(string.Format("{0}", node.DataType));
                        //parentSP.AppendChild(emptySP);
                        //element.AppendChild(parentSP);

                        var structIndex = (ushort) r.ReadUInt32();
                        var recordIndex = (int) r.ReadUInt32();

                        Console.WriteLine($"Require ClassMapping for struct {getStructDefinition(structIndex).Name}");
            //this.DocumentRoot.Require_ClassMapping.Add(new ClassMapping
            //                                           {
            //                                             Node = emptySP,
            //                                             StructIndex =
            //                                               (ushort) this._br.ReadUInt32(),
            //                                             RecordIndex = (int) this._br.ReadUInt32()
            //                                           });

          }
          else
          {
            node.Read();

          }
        }
        else
        {
          // ConversionType seems only used to differentiate between arrays and single values
          var arrayCount = r.ReadUInt32();
          var firstIndex = r.ReadUInt32();

          var child = this.DocumentRoot.CreateElement(node.Name);

          var elements = new List<object>();

          for (var i = 0; i < arrayCount; i++)
          {
            switch (node.DataType)
            {
              case DataType.Boolean:
                child.AppendChild(this.DocumentRoot.Array_BooleanValues[firstIndex + i]
                                      .Read());
                break;
              case DataType.Double:
                child.AppendChild(this.DocumentRoot.Array_DoubleValues[firstIndex + i]
                                      .Read());
                break;
              case DataType.Enum:
                child.AppendChild(this.DocumentRoot.Array_EnumValues[firstIndex + i]
                                      .Read());
                break;
              case DataType.Guid:
                child.AppendChild(this.DocumentRoot.Array_GuidValues[firstIndex + i]
                                      .Read());
                break;
              case DataType.Int16:
                child.AppendChild(this.DocumentRoot.Array_Int16Values[firstIndex + i]
                                      .Read());
                break;
              case DataType.Int32:
                child.AppendChild(this.DocumentRoot.Array_Int32Values[firstIndex + i]
                                      .Read());
                break;
              case DataType.Int64:
                child.AppendChild(this.DocumentRoot.Array_Int64Values[firstIndex + i]
                                      .Read());
                break;
              case DataType.SByte:
                child.AppendChild(this.DocumentRoot.Array_Int8Values[firstIndex + i]
                                      .Read());
                break;
              case DataType.Locale:
                child.AppendChild(this.DocumentRoot.Array_LocaleValues[firstIndex + i]
                                      .Read());
                break;
              case DataType.Reference:
                child.AppendChild(this.DocumentRoot.Array_ReferenceValues[firstIndex + i]
                                      .Read());
                break;
              case DataType.Single:
                child.AppendChild(this.DocumentRoot.Array_SingleValues[firstIndex + i]
                                      .Read());
                break;
              case DataType.String:
                child.AppendChild(this.DocumentRoot.Array_StringValues[firstIndex + i]
                                      .Read());
                break;
              case DataType.UInt16:
                child.AppendChild(this.DocumentRoot.Array_UInt16Values[firstIndex + i]
                                      .Read());
                break;
              case DataType.UInt32:
                child.AppendChild(this.DocumentRoot.Array_UInt32Values[firstIndex + i]
                                      .Read());
                break;
              case DataType.UInt64:
                child.AppendChild(this.DocumentRoot.Array_UInt64Values[firstIndex + i]
                                      .Read());
                break;
              case DataType.Byte:
                child.AppendChild(this.DocumentRoot.Array_UInt8Values[firstIndex + i]
                                      .Read());
                break;
              case DataType.Class:
                var emptyC = this.DocumentRoot.CreateElement(string.Format("{0}", node.DataType));
                child.AppendChild(emptyC);
                this.DocumentRoot.Require_ClassMapping.Add(new ClassMapping
                                                           {
                                                             Node = emptyC,
                                                             StructIndex = node.StructIndex,
                                                             RecordIndex = firstIndex + i
                                                           });
                break;
              case DataType.StrongPointer:
                var emptySP = this.DocumentRoot.CreateElement(string.Format("{0}", node.DataType));
                child.AppendChild(emptySP);
                this.DocumentRoot.Require_StrongMapping.Add(new ClassMapping
                                                            {
                                                              Node = emptySP,
                                                              StructIndex = node.StructIndex,
                                                              RecordIndex = firstIndex + i
                                                            });
                break;
              case DataType.WeakPointer:
                var weakPointerElement = this.DocumentRoot.CreateElement("WeakPointer");
                var weakPointerAttribute = this.DocumentRoot.CreateAttribute(node.Name);

                weakPointerElement.Attributes.Append(weakPointerAttribute);
                child.AppendChild(weakPointerElement);

                this.DocumentRoot.Require_WeakMapping1.Add(new ClassMapping
                                                           {
                                                             Node = weakPointerAttribute,
                                                             StructIndex = node.StructIndex,
                                                             RecordIndex = firstIndex + i
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

          element.AppendChild(child);
        }
      }

      attribute = this.DocumentRoot.CreateAttribute("__type");
      attribute.Value = baseStruct.Name;
      element.Attributes.Append(attribute);

      if (ParentTypeIndex != 0xFFFFFFFF)
      {
        attribute = this.DocumentRoot.CreateAttribute("__polymorphicType");
        attribute.Value = Name;
        element.Attributes.Append(attribute);
      }

      return element;
    }
  }
}
