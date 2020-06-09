using System;
using System.IO;

namespace Scope.FileViewer.DataForge.Models
{
  internal class StringLookup
  {
    private readonly Func<uint, string> _valueOf;
    private uint _value;

    public StringLookup(BinaryReader r, Func<uint, string> valueOf)
    {
      _value = r.ReadUInt32();
      _valueOf = valueOf;
    }

    string Value => _valueOf(_value);
  }
}