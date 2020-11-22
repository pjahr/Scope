using System;
using System.IO;

namespace Scope.FileViewer.DataForge.Models
{
  public class Reference
  {
    public Reference(BinaryReader r)
    {
      Item = r.ReadInt32();
      Value = r.ReadGuid();
    }

    public int Item { get; }
    public Guid Value { get; }
  }
}