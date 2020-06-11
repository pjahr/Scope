using System;
using System.Collections.Generic;
using System.IO;

namespace Scope.FileViewer.DataForge.Models
{
  internal class DataForgeFile
  {
    private bool IsLegacy { get; }
    private int FileVersion { get; }

    public StructDefinition[] StructDefinitionTable { get; }
    public PropertyDefinition[] PropertyDefinitionTable { get; }
    public EnumDefinition[] EnumDefinitionTable { get; }
    private DataMapping[] DataMappingTable { get; }
    private Record[] RecordDefinitionTable { get; }
    private StringLookup[] EnumOptionTable { get; }

    private string[] ValueTable { get; }
    public Dictionary<string, string> Files { get; }
    public Reference[] ReferenceValues { get; }
    public Guid[] GuidValues { get; }
    public StringLookup[] StringValues { get; }
    public uint[] LocaleValues { get; }
    public uint[] EnumValues { get; }
    public sbyte[] Int8Values { get; }
    public short[] Int16Values { get; }
    public int[] Int32Values { get; }
    public long[] Int64Values { get; }
    public byte[] UInt8Values { get; }
    public ushort[] UInt16Values { get; }
    public uint[] UInt32Values { get; }
    public ulong[] UInt64Values { get; }
    public bool[] BooleanValues { get; }
    public float[] SingleValues { get; }
    public double[] DoubleValues { get; }
    public Pointer[] StrongValues { get; }
    public Pointer[] WeakValues { get; }

    public Dictionary<uint, string> ValueMap { get; }

    public List<ClassMapping> ClassMappings { get; }
    public List<ClassMapping> StrongMappings { get; }
    public List<ClassMapping> WeakMappings1 { get; }
    public List<ClassMapping> WeakMappings2 { get; }

    public DataForgeFile(BinaryReader r, bool legacy = false)
    {
      IsLegacy = legacy;

      var temp00 = r.ReadInt32(); // ?

      FileVersion = r.ReadInt32();

      ClassMappings = new List<ClassMapping>();
      StrongMappings = new List<ClassMapping>();
      WeakMappings1 = new List<ClassMapping>();
      WeakMappings2 = new List<ClassMapping>();

      if (!IsLegacy)
      {
        var atemp1 = r.ReadUInt16();
        var atemp2 = r.ReadUInt16();
        var atemp3 = r.ReadUInt16();
        var atemp4 = r.ReadUInt16();
      }

      // item counts
      var structDefinitionCount = r.ReadInt32();
      var propertyDefinitionCount = r.ReadInt32();
      var enumDefinitionCount = r.ReadInt32();
      var dataMappingCount = r.ReadInt32();
      var recordDefinitionCount = r.ReadInt32();

      var booleanValueCount = r.ReadInt32();
      var int8ValueCount = r.ReadInt32();
      var int16ValueCount = r.ReadInt32();
      var int32ValueCount = r.ReadInt32();
      var int64ValueCount = r.ReadInt32();
      var uint8ValueCount = r.ReadInt32();
      var uint16ValueCount = r.ReadInt32();
      var uint32ValueCount = r.ReadInt32();
      var uint64ValueCount = r.ReadInt32();

      var singleValueCount = r.ReadInt32();
      var doubleValueCount = r.ReadInt32();
      var guidValueCount = r.ReadInt32();
      var stringValueCount = r.ReadInt32();
      var localeValueCount = r.ReadInt32();
      var enumValueCount = r.ReadInt32();
      var strongValueCount = r.ReadInt32();
      var weakValueCount = r.ReadInt32();

      var referenceValueCount = r.ReadInt32();
      var enumOptionCount = r.ReadInt32();
      var textLength = r.ReadUInt32();
      var unknown = (IsLegacy) ? 0 : r.ReadUInt32();

      StructDefinitionTable = structDefinitionCount.ToArray(() => new StructDefinition(r, V));
      
      PropertyDefinitionTable = propertyDefinitionCount.ToArray(() => new PropertyDefinition(r, V));
      
      EnumDefinitionTable = enumDefinitionCount.ToArray(() => new EnumDefinition(r, V, i=>EnumOptionTable[i].Value));
      
      DataMappingTable = dataMappingCount.ToArray(() => new DataMapping(r, V));
      
      RecordDefinitionTable = recordDefinitionCount.ToArray(() => new Record(r, V));

      Int8Values = int8ValueCount.ToArray(r.ReadSByte);
      Int16Values = int16ValueCount.ToArray(r.ReadInt16);
      Int32Values = int32ValueCount.ToArray(r.ReadInt32);
      Int64Values = int64ValueCount.ToArray(r.ReadInt64);
      UInt8Values = uint8ValueCount.ToArray(r.ReadByte);
      UInt16Values = uint16ValueCount.ToArray(r.ReadUInt16);
      UInt32Values = uint32ValueCount.ToArray(r.ReadUInt32);
      UInt64Values = uint64ValueCount.ToArray(r.ReadUInt64);
      BooleanValues = booleanValueCount.ToArray(r.ReadBoolean);
      SingleValues = singleValueCount.ToArray(r.ReadSingle);
      DoubleValues = doubleValueCount.ToArray(r.ReadDouble);

      GuidValues = guidValueCount.ToArray(r.ReadGuid);
      StringValues = stringValueCount.ToArray(()=>new StringLookup(r,V));
      LocaleValues = localeValueCount.ToArray(r.ReadUInt32);
      EnumValues = enumValueCount.ToArray(r.ReadUInt32);
      StrongValues = strongValueCount.ToArray(() => new Pointer(r));
      WeakValues = weakValueCount.ToArray(() => new Pointer(r));

      ReferenceValues = referenceValueCount.ToArray(() => new Reference(r));
      EnumOptionTable = enumOptionCount.ToArray(() => new StringLookup(r, V));

      var values = new List<string>();

      var maxPosition = r.BaseStream.Position + textLength;
      var startPosition = r.BaseStream.Position;

      ValueMap = new Dictionary<uint, string>();

      while (r.BaseStream.Position < maxPosition)
      {
        var offset = r.BaseStream.Position - startPosition;
        var value = r.ReadNullTerminatedString();
        values.Add(value);
        ValueMap[(uint)offset] = value;
      }

      for (var i1 = 0; i1 < DataMappingTable.Length; i1++)
      {
        var dataMapping = this.DataMappingTable[i1];
        var dataStruct = StructDefinitionTable[dataMapping.StructIndex];

        for (var i = 0; i < dataMapping.StructCount; i++)
        {
          dataStruct.Read(r, dataMapping.Name, this);
        }
      }

      ValueTable = values.ToArray();

      Files = new Dictionary<string, string>();

      foreach (var record in RecordDefinitionTable)
      {
        var filename = ValueMap[record.FileNameOffset];
        var name = ValueMap[record.NameOffset];
        if (Files.ContainsKey(filename))
        {
          Console.WriteLine($"{filename} {name} {record.OtherIndex} {record.StructIndex} {record.VariantIndex}");
        }
        else
        {
          Files.Add(filename, name);
        }
      }
    }

    private string V(uint id) => ValueMap[id];
  }
}
