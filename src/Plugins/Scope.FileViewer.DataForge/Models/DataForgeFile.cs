using System;
using System.Collections.Generic;
using System.Diagnostics;
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
      
      DataMappingTable = dataMappingCount.ToArray(() => new DataMapping(r, VS));
      
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

      ///////////////////////////////////////////////////////////////////////////

      //r.EnableLog();

      foreach (var dataMapping in DataMappingTable)
      {
        var dataStruct = StructDefinitionTable[dataMapping.StructIndex];

        Console.WriteLine($"Map {dataMapping.Name}->{dataStruct.Name} ({dataMapping.StructCount})");
        
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


    // TODO: Explain Shortcuts
    private string V(uint id) => ValueMap[id];
    private string VS(uint id) => ValueMap[StructDefinitionTable[id].NameOffset];
  }

  public class SpyBinaryReader : BinaryReader
  {
    private List<Tuple<string, ReadType>> _acess = new List<Tuple<string, ReadType>>();

    private Action<ReadType> _log = (_) => { };

    public SpyBinaryReader(Stream input) : base(input) { }

    public void EnableLog()
    {
      //_log = (accessType) => _acess.Add(new Tuple<string, ReadType>(new StackFrame(4).GetMethod().DeclaringType.Name, accessType));
      _log = (accessType) =>
      {
        Console.WriteLine($"{accessType} [{BaseStream.Position}]");
        if (BaseStream.Position == 12537565)
        {
          // debug stop

        }
      };
    }

    public override int Read()
    {
      _log(ReadType.None);
      return base.Read();
    }


    public override bool ReadBoolean()
    {
      _log(ReadType.Bool);
      return base.ReadBoolean();
    }

    public override byte ReadByte()
    {
      _log(ReadType.Byte);
      return base.ReadByte();
    }

    public override int Read(byte[] buffer, int index, int count)
    {
      _log(ReadType.ByteBuffer);
      return base.Read(buffer, index, count);
    }

    public override int Read(char[] buffer, int index, int count)
    {
      _log(ReadType.CharBuffer);
      return base.Read(buffer, index, count);
    }

    public override byte[] ReadBytes(int count)
    {
      _log(ReadType.Bytes);
      return base.ReadBytes(count);
    }

    public override char ReadChar()
    {
      _log(ReadType.Char);
      return base.ReadChar();
    }

    public override char[] ReadChars(int count)
    {
      _log(ReadType.Chars);
      return base.ReadChars(count);
    }

    public override decimal ReadDecimal()
    {
      _log(ReadType.Decimal);
      return base.ReadDecimal();
    }

    public override double ReadDouble()
    {
      _log(ReadType.Double);
      return base.ReadDouble();
    }

    public override short ReadInt16()
    {
      _log(ReadType.Int16);
      return base.ReadInt16();
    }

    public override int ReadInt32()
    {
      _log(ReadType.Int32);
      return base.ReadInt32();
    }

    public override long ReadInt64()
    {
      _log(ReadType.Int64);
      return base.ReadInt64();
    }

    public override sbyte ReadSByte()
    {
      _log(ReadType.Sbyte);
      return base.ReadSByte();
    }

    public override float ReadSingle()
    {
      _log(ReadType.Single);
      return base.ReadSingle();
    }

    public override string ReadString()
    {
      _log(ReadType.String);
      return base.ReadString();
    }

    public override ushort ReadUInt16()
    {
      _log(ReadType.UInt16);
      return base.ReadUInt16();
    }

    public override uint ReadUInt32()
    {
      _log(ReadType.UInt32);
      return base.ReadUInt32();
    }

    public override ulong ReadUInt64()
    {
      _log(ReadType.UInt64);
      return base.ReadUInt64();
    }

  }

  public enum ReadType
  {
    None,
    Bool,
    Byte,
    ByteBuffer,
    CharBuffer,
    Bytes,
    Char,
    Chars,
    Decimal,
    Double,
    Int16,
    Int32,
    Int64,
    Sbyte,
    Single,
    String,
    UInt16,
    UInt32,
    UInt64

  }

}
