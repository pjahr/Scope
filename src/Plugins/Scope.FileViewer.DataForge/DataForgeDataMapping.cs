using System.IO;

namespace Scope.FileViewer.DataForge
{
  internal class DataForgeDataMapping
  {
    public DataForgeDataMapping(BinaryReader r)
    {
      StructCount = r.ReadUInt16();
      StructIndex = r.ReadUInt16();
    }

    public ushort StructIndex { get; }
    public ushort StructCount { get; }
    public uint NameOffset { get; }

    //NameOffset = documentRoot.StructDefinitionTable[StructIndex].NameOffset;
    //public String Name { get { return this.DocumentRoot.ValueMap[this.NameOffset]; } }
  }
}