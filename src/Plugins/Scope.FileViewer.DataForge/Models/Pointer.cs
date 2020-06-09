using System.IO;

namespace Scope.FileViewer.DataForge.Models
{
  internal class Pointer
  {
    public Pointer(BinaryReader r)
    {
      StructType = r.ReadUInt32();
      Index = r.ReadUInt32();
    }

    public uint StructType { get; }
    public uint Index { get; }
  }
}