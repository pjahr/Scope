using System;
using System.IO;

namespace Scope.FileViewer.DataForge.Models
{
  public class DataMapping
  {
    private readonly Func<uint, string> _valueOf;

    public DataMapping(BinaryReader r, Func<uint, string> valueOf, int fileVersion)
    {
      _valueOf = valueOf;

      if (fileVersion >= 5)
      {
        StructCount = r.ReadUInt32();
        StructIndex = r.ReadUInt32();
      }
      else
      {
        StructCount = r.ReadUInt16();
        StructIndex = r.ReadUInt16();
      }

    }

    public uint StructIndex { get; }
    public uint StructCount { get; }

    public string Name => _valueOf(StructIndex);

    public override string ToString()
    {
      return $"{Name} I{StructIndex} C{StructCount}";
    }
  }
}