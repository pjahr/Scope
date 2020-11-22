using System;
using System.IO;

namespace Scope.FileViewer.DataForge.Models
{
  public class StringLookup
  {
    private readonly Func<uint, string> _valueOf;
    private uint _value;

    public StringLookup(BinaryReader r, Func<uint, string> valueOf)
    {
      _value = r.ReadUInt32();
      _valueOf = valueOf;
    }

    internal string Value => _valueOf(_value);
  }
}