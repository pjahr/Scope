using System;
using System.IO;

namespace Scope.FileViewer.DataForge.Models
{
  internal class DataMapping
  {
    private readonly Func<uint, string> _valueOf;

    public DataMapping(BinaryReader r, Func<uint, string> valueOf)
    {
      _valueOf = valueOf;

      StructCount = r.ReadUInt16();
      StructIndex = r.ReadUInt16();
    }

    public ushort StructIndex { get; }
    public ushort StructCount { get; }
    public uint NameOffset { get; }

    //NameOffset = documentRoot.StructDefinitionTable[StructIndex].NameOffset;
    public string Name => _valueOf(NameOffset);
  }
}