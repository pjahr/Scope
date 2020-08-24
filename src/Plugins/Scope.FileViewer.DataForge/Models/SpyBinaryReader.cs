using System;
using System.Collections.Generic;
using System.IO;

namespace Scope.FileViewer.DataForge.Models
{
  /// <summary>
  /// This is a debug stand-in for the BinearyReader used during deserialization
  /// of the dcb file that is very vocal (and therefore slow) about what it is
  /// currently doing.
  /// </summary>
  public class SpyBinaryReader : BinaryReader
  {
    private List<Tuple<string, ReadType>> _acess = new List<Tuple<string, ReadType>>();

    private Action<ReadType> _log = (_) => { };

    public SpyBinaryReader(Stream input) : base(input) { }

    // to enable the log you have to change the dependency of DataForgeFile to this type explicitely and call this method once.
    public void EnableLog()
    {
      _log = (accessType) =>
      {
        Console.WriteLine($"{accessType} [{BaseStream.Position}]");
        //if (BaseStream.Position == 12537565)
        //{
        //  // use this for breakpoints at specific stream locations...

        //}
      };
    }

    public override int Read()
    {
      _log(ReadType.None);
      return base.Read();
    }


    public override bool ReadBoolean()
    {
      _log(ReadType.Bool);
      return base.ReadBoolean();
    }

    public override byte ReadByte()
    {
      _log(ReadType.Byte);
      return base.ReadByte();
    }

    public override int Read(byte[] buffer, int index, int count)
    {
      _log(ReadType.ByteBuffer);
      return base.Read(buffer, index, count);
    }

    public override int Read(char[] buffer, int index, int count)
    {
      _log(ReadType.CharBuffer);
      return base.Read(buffer, index, count);
    }

    public override byte[] ReadBytes(int count)
    {
      _log(ReadType.Bytes);
      return base.ReadBytes(count);
    }

    public override char ReadChar()
    {
      _log(ReadType.Char);
      return base.ReadChar();
    }

    public override char[] ReadChars(int count)
    {
      _log(ReadType.Chars);
      return base.ReadChars(count);
    }

    public override decimal ReadDecimal()
    {
      _log(ReadType.Decimal);
      return base.ReadDecimal();
    }

    public override double ReadDouble()
    {
      _log(ReadType.Double);
      return base.ReadDouble();
    }

    public override short ReadInt16()
    {
      _log(ReadType.Int16);
      return base.ReadInt16();
    }

    public override int ReadInt32()
    {
      _log(ReadType.Int32);
      return base.ReadInt32();
    }

    public override long ReadInt64()
    {
      _log(ReadType.Int64);
      return base.ReadInt64();
    }

    public override sbyte ReadSByte()
    {
      _log(ReadType.Sbyte);
      return base.ReadSByte();
    }

    public override float ReadSingle()
    {
      _log(ReadType.Single);
      return base.ReadSingle();
    }

    public override string ReadString()
    {
      _log(ReadType.String);
      return base.ReadString();
    }

    public override ushort ReadUInt16()
    {
      _log(ReadType.UInt16);
      return base.ReadUInt16();
    }

    public override uint ReadUInt32()
    {
      _log(ReadType.UInt32);
      return base.ReadUInt32();
    }

    public override ulong ReadUInt64()
    {
      _log(ReadType.UInt64);
      return base.ReadUInt64();
    }

  }

}
