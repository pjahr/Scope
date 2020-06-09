using System;
using System.Collections.Generic;
using System.IO;

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


      var properties = new List<PropertyDefinition> { };

      if (false)
      {
        var arrayCount = r.ReadUInt32();
        var firstIndex = r.ReadUInt32();
      }
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
  }
}