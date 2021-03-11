﻿using System;
using System.IO;

namespace Scope.FileViewer.DataForge.Models
{
  //TODO: duplicate of EnumValue
  public class StringLookup
  {
    private readonly Func<uint, string> _valueOf;
    private readonly uint _value;

    public StringLookup(BinaryReader r, Func<uint, string> valueOf)
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