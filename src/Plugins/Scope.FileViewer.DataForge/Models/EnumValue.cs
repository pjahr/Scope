using System;
using System.IO;

namespace Scope.FileViewer.DataForge.Models
{
  public class EnumValue
  {
    private readonly Func<uint, string> _valueOf;
    private readonly uint _value;

    public EnumValue(BinaryReader r, Func<uint, string> valueOf)
    {
      _value = r.ReadUInt32();
      _valueOf = valueOf;
    }

    internal string Value => _valueOf(_value);

    public override string ToString()
    {
      return Value;
    }
  }
}