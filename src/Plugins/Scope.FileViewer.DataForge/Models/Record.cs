using System;
using System.IO;

namespace Scope.FileViewer.DataForge.Models
{
  internal class Record
  {
    private readonly Func<uint, string> _valueOf;

    public Record(BinaryReader r, Func<uint, string> valueOf)
    {
      _valueOf = valueOf;

      NameOffset     = r.ReadUInt32();
      FileNameOffset = r.ReadUInt32();
      StructIndex    = r.ReadUInt32();
      Hash           = r.ReadGuid();
      VariantIndex   = r.ReadUInt16();
      OtherIndex     = r.ReadUInt16();
    }

    public uint   NameOffset { get; set; }
    public uint   FileNameOffset { get; set; }
    public uint   StructIndex { get; set; }
    public Guid?  Hash { get; set; }
    public ushort VariantIndex { get; set; }
    public ushort OtherIndex { get; set; }

    public string Name => _valueOf(NameOffset);
    public string FileName => _valueOf(FileNameOffset);
    //public String __structIndex { get { return String.Format("{0:X4}", StructIndex); } }
    //public String __variantIndex { get { return String.Format("{0:X4}", VariantIndex); } }
    //public String __otherIndex { get { return String.Format("{0:X4}", OtherIndex); } }
  }
}