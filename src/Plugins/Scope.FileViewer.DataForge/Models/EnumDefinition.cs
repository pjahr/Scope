using System;
using System.IO;

namespace Scope.FileViewer.DataForge.Models
{
  internal class EnumDefinition
  {
    private readonly Func<uint, string> _valueOf;

    public EnumDefinition(BinaryReader r, Func<uint, string> valueOf)
    {
      _valueOf = valueOf;

      NameOffset = r.ReadUInt32();
      ValueCount = r.ReadUInt16();
      FirstValueIndex = r.ReadUInt16();
    }

    public uint NameOffset { get; }
    public ushort ValueCount { get; }
    public ushort FirstValueIndex { get; }

    public string Name => _valueOf(NameOffset);
  }
}