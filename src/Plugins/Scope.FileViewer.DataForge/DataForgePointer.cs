using System.IO;

namespace Scope.FileViewer.DataForge
{
  internal class DataForgePointer
  {
    public DataForgePointer(BinaryReader r)
    {
      StructType = r.ReadUInt32();
      Index = r.ReadUInt32();
    }

    public uint StructType { get; }
    public uint Index { get; }
  }
}