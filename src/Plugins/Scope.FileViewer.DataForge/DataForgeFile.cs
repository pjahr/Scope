using System;
using System.Collections.Generic;
using System.IO;

namespace Scope.FileViewer.DataForge
{
  internal class DataForgeFile
  {
    private bool IsLegacy { get; }
    private int FileVersion { get; }

    private DataForgeStructDefinition[] StructDefinitionTable { get; }
    private DataForgePropertyDefinition[] PropertyDefinitionTable { get; }
    private DataForgeEnumDefinition[] EnumDefinitionTable { get; }
    private DataForgeDataMapping[] DataMappingTable { get; }
    private DataForgeRecord[] RecordDefinitionTable { get; }
    private uint[] EnumOptionTable { get; }
    private string[] ValueTable { get; }

    private DataForgeReference[] ReferenceValues { get; }
    private Guid[] GuidValues { get; }
    private uint[] StringValues { get; }
    private uint[] LocaleValues { get; }
    private uint[] EnumValues { get; }
    private sbyte[] Int8Values { get; }
    private short[] Int16Values { get; }
    private int[] Int32Values { get; }
    private long[] Int64Values { get; }
    private byte[] UInt8Values { get; }
    private ushort[] UInt16Values { get; }
    private uint[] UInt32Values { get; }
    private ulong[] UInt64Values { get; }
    private bool[] BooleanValues { get; }
    private float[] SingleValues { get; }
    private double[] DoubleValues { get; }
    private DataForgePointer[] StrongValues { get; }
    private DataForgePointer[] WeakValues { get; }

    private Dictionary<uint, string> ValueMap { get; }

    private List<ClassMapping> ClassMappings { get; }
    private List<ClassMapping> StrongMappings { get; }
    private List<ClassMapping> WeakMappings1 { get; }
    private List<ClassMapping> WeakMappings2 { get; }

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

      StructDefinitionTable = structDefinitionCount.ToArray(() => new DataForgeStructDefinition(r));
      PropertyDefinitionTable = propertyDefinitionCount.ToArray(() => new DataForgePropertyDefinition(r));
      EnumDefinitionTable = enumDefinitionCount.ToArray(() => new DataForgeEnumDefinition(r));
      DataMappingTable = dataMappingCount.ToArray(() => new DataForgeDataMapping(r));
      RecordDefinitionTable = recordDefinitionCount.ToArray(() => new DataForgeRecord(r));

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
      StringValues = stringValueCount.ToArray(r.ReadUInt32);
      LocaleValues = localeValueCount.ToArray(r.ReadUInt32);
      EnumValues = enumValueCount.ToArray(r.ReadUInt32);
      StrongValues = strongValueCount.ToArray(() => new DataForgePointer(r));
      WeakValues = weakValueCount.ToArray(() => new DataForgePointer(r));

      ReferenceValues = referenceValueCount.ToArray(() => new DataForgeReference(r));
      EnumOptionTable = enumOptionCount.ToArray(r.ReadUInt32);

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

      ValueTable = values.ToArray();
    }
  }
}
