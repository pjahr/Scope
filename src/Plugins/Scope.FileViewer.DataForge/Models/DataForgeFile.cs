using Scope.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Scope.FileViewer.DataForge.Models
{

  /// <summary>
  /// Currently this is just a copy of allurans code adjusted to my coding style... 
  /// </summary>
  public class DataForgeFile
  {
    private IMessageQueue _messages;

    private bool IsLegacy { get; }
    private int FileVersion { get; }

    public StructDefinition[] StructDefinitionTable { get; private set; }
    public PropertyDefinition[] PropertyDefinitionTable { get; private set; }
    public EnumDefinition[] EnumDefinitionTable { get; private set; }
    private DataMapping[] DataMappingTable { get; set; }
    private Record[] RecordDefinitionTable { get; set; }
    private StringLookup[] EnumOptionTable { get; set; }

    private string[] ValueTable { get; set; }
    public Dictionary<string, string> Files { get; set; }
    public Reference[] ReferenceValues { get; set; }
    public Guid[] GuidValues { get; set; }
    public StringLookup[] StringValues { get; set; }
    public uint[] LocaleValues { get; set; }
    public uint[] EnumValues { get; set; }
    public sbyte[] Int8Values { get; set; }
    public short[] Int16Values { get; set; }
    public int[] Int32Values { get; set; }
    public long[] Int64Values { get; set; }
    public byte[] UInt8Values { get; set; }
    public ushort[] UInt16Values { get; set; }
    public uint[] UInt32Values { get; set; }
    public ulong[] UInt64Values { get; set; }
    public bool[] BooleanValues { get; set; }
    public float[] SingleValues { get; set; }
    public double[] DoubleValues { get; set; }
    public Pointer[] StrongValues { get; set; }
    public Pointer[] WeakValues { get; set; }

    public Dictionary<uint, string> ValueMap { get; set; }
    public Dictionary<uint, List<Struct>> DataMap { get; set; }

    public List<ClassMapping> ClassMappings { get; set; }
    public List<ClassMapping> StrongMappings { get; set; }
    public List<ClassMapping> WeakMappings1 { get; set; }
    public List<ClassMapping> WeakMappings2 { get; set; }

    public DataForgeFile(BinaryReader r,
                         IMessageQueue messages,
                         bool legacy = false)
    {
      _messages = messages;

      IsLegacy = legacy;

      var temp00 = r.ReadInt32(); // ?

      FileVersion = r.ReadInt32();

      ClassMappings = new List<ClassMapping>();
      StrongMappings = new List<ClassMapping>();
      WeakMappings1 = new List<ClassMapping>();
      WeakMappings2 = new List<ClassMapping>();
      DataMap = new Dictionary<uint, List<Struct>>();

      if (!IsLegacy) // seams to be obsolete
      {
        var atemp1 = r.ReadUInt16();
        var atemp2 = r.ReadUInt16();
        var atemp3 = r.ReadUInt16();
        var atemp4 = r.ReadUInt16();
      }

      // read all the item counts
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

      var unknown = (IsLegacy) ? 0 : r.ReadUInt32(); // obsolete?

      // read all the item types using the respective item counts

      Profile(() => StructDefinitionTable = structDefinitionCount.ToArray(() => new StructDefinition(r, V)), "Reading structs");


      Profile(() => PropertyDefinitionTable = propertyDefinitionCount.ToArray(() => new PropertyDefinition(r, V)), "Reading properties");

      Profile(() => EnumDefinitionTable = enumDefinitionCount.ToArray(() => new EnumDefinition(r, V, i => EnumOptionTable[i].Value)), "Reading enums");

      Profile(() => DataMappingTable = dataMappingCount.ToArray(() => new DataMapping(r, VS)), "Reading data mappings");

      Profile(() => RecordDefinitionTable = recordDefinitionCount.ToArray(() => new Record(r, V)), "Reading records");

      Profile(() => Int8Values = int8ValueCount.ToArray(r.ReadSByte), "Reading Int8");
      Profile(() => Int16Values = int16ValueCount.ToArray(r.ReadInt16), "Reading Int16");
      Profile(() => Int32Values = int32ValueCount.ToArray(r.ReadInt32), "Reading Int32");
      Profile(() => Int64Values = int64ValueCount.ToArray(r.ReadInt64), "Reading Int64");
      Profile(() => UInt8Values = uint8ValueCount.ToArray(r.ReadByte), "Reading UInt8");
      Profile(() => UInt16Values = uint16ValueCount.ToArray(r.ReadUInt16), "Reading UInt16");
      Profile(() => UInt32Values = uint32ValueCount.ToArray(r.ReadUInt32), "Reading UInt32");
      Profile(() => UInt64Values = uint64ValueCount.ToArray(r.ReadUInt64), "Reading UInt64");
      Profile(() => BooleanValues = booleanValueCount.ToArray(r.ReadBoolean), "Reading Booleans");
      Profile(() => SingleValues = singleValueCount.ToArray(r.ReadSingle), "Reading Singles");
      Profile(() => DoubleValues = doubleValueCount.ToArray(r.ReadDouble), "Reading Doubles");

      Profile(() => GuidValues = guidValueCount.ToArray(r.ReadGuid), "Reading Guids");
      Profile(() => StringValues = stringValueCount.ToArray(() => new StringLookup(r, V)), "Reading Strings");
      Profile(() => LocaleValues = localeValueCount.ToArray(r.ReadUInt32), "Reading Locales");
      Profile(() => EnumValues = enumValueCount.ToArray(r.ReadUInt32), "Reading Enum values");
      Profile(() => StrongValues = strongValueCount.ToArray(() => new Pointer(r)), "Reading strong pointers");
      Profile(() => WeakValues = weakValueCount.ToArray(() => new Pointer(r)), "Reading weak pointers");

      Profile(() => ReferenceValues = referenceValueCount.ToArray(() => new Reference(r)), "Reading referenes");
      Profile(() => EnumOptionTable = enumOptionCount.ToArray(() => new StringLookup(r, V)), "Reading enum options");
      
      Profile(() => { ReadValues(r, textLength); }, "Reading value map");

      Profile(() => { MapData(r); }, "Mapping data");
      Profile(() => { GenerateFiles(); }, "Generating files");
    }

    private void ReadValues(BinaryReader r, uint textLength)
    {
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

    private void MapData(BinaryReader r)
    {
      var structs = new List<Struct>();
      Console.WriteLine(structs.Count);
      foreach (var dataMapping in DataMappingTable)
      {
        DataMap[dataMapping.StructIndex] = new List<Struct>();
        var dataStruct = StructDefinitionTable[dataMapping.StructIndex];

        for (var i = 0; i < dataMapping.StructCount; i++)
        {
          DataMap[dataMapping.StructIndex].Add(dataStruct.Read(r, dataMapping.Name, this));
        }
      }

      foreach (var dataMapping in ClassMappings)
      {
        if (dataMapping.StructIndex == 0xFFFF)
        {
          throw new NotImplementedException("???");
          //dataMapping.Item1.ParentNode.ReplaceChild(
          //    this._xmlDocument.CreateElement("null"),
          //    dataMapping.Item1);

        }
        else if (this.DataMap.ContainsKey(dataMapping.StructIndex) && this.DataMap[dataMapping.StructIndex].Count > dataMapping.RecordIndex)
        {
          //dataMapping.Node.ParentNode.ReplaceChild(
          //    this.DataMap[dataMapping.StructIndex][dataMapping.RecordIndex],
          //    dataMapping.Node);
        }
        else
        {
          throw new NotImplementedException("bug");          
        }
      }
    }

    private void GenerateFiles()
    {

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

    private void Profile(Action action, string what)
    {
      var sw = Stopwatch.StartNew();
      action();
      _messages.Add($"DataForge: {what} took {sw.ElapsedMilliseconds} ms.");
    }
  }
}
