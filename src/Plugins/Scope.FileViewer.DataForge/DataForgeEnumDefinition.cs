using System.IO;

namespace Scope.FileViewer.DataForge
{
  internal class DataForgeEnumDefinition
  {
    public DataForgeEnumDefinition(BinaryReader r)
    {
      NameOffset = r.ReadUInt32();
      ValueCount = r.ReadUInt16();
      FirstValueIndex = r.ReadUInt16();
    }

    public uint NameOffset { get; }
    public ushort ValueCount { get; }
    public ushort FirstValueIndex { get; }

    //public String Name { get { return this.DocumentRoot.ValueMap[this.NameOffset]; } }
  }
}