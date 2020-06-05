using System;
using System.IO;

namespace Scope.FileViewer.DataForge
{
  internal class DataForgeReference
  {
    public DataForgeReference(BinaryReader r)
    {
      Item = r.ReadInt32();
      Value = r.ReadGuid();
    }

    public int Item { get; }
    public Guid Value { get; }
  }
}