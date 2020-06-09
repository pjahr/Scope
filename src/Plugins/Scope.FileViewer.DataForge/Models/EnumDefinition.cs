using System;
using System.Collections.Generic;
using System.IO;

namespace Scope.FileViewer.DataForge.Models
{
  internal class EnumDefinition
  {
    private readonly Func<uint, string> _valueOf;
    private readonly Func<uint, string> _enumValueOf;

    public EnumDefinition(BinaryReader r, Func<uint, string> valueOf, Func<uint, string> enumValueOf)
    {
      _valueOf = valueOf;
      _enumValueOf = enumValueOf;

      NameOffset = r.ReadUInt32();
      ValueCount = r.ReadUInt16();
      FirstValueIndex = r.ReadUInt16();
    }

    public uint NameOffset { get; }
    public ushort ValueCount { get; }
    public ushort FirstValueIndex { get; }

    public string Name => _valueOf(NameOffset);
    public IEnumerable<string> Values
    {
      get
      {
        for (uint i = FirstValueIndex, j = (uint)(FirstValueIndex + ValueCount); i < j; i++)
        {
          yield return _enumValueOf(i);
        }
      }
    }

    public override string ToString() => $"{Name}";
  }
}