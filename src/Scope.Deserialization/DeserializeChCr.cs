using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Scope.Deserialization
{
  public static class DeserializeChCr
  {
    public static string[] Deserialize(string file)
    {
      return Deserialize(File.OpenRead(file));
    }

    public static string[] Deserialize(Stream stream)
    {
      uint numberOfChunks = 0;
      var chunks = new List<Chunk>();

      using (BinaryReader r = new BinaryReader(stream))
      {
        stream.Position = 0;

        var signature = r.ReadUInt32();
        var version = r.ReadUInt32();
        numberOfChunks = r.ReadUInt32();
        var chunkTableOffset = r.ReadUInt32();

        stream.Seek(chunkTableOffset, SeekOrigin.Begin);
        for (int i = 0; i < numberOfChunks; i++)
        {
          chunks.Add(new Chunk(r));
        }

        foreach (var chunk in chunks)
        {
          chunk.Read(r);
        }
      }

      return chunks.Where(c => c.Type == ChunkType.Json)
                   .Select(c => c.Content)
                   .ToArray();
    }

    private class Chunk
    {
      public Chunk(BinaryReader r)
      {
        Type = (ChunkType)r.ReadUInt16();
        Version = r.ReadUInt16();
        Id = r.ReadUInt32();
        Size = Convert.ToInt32(r.ReadUInt32());
        Offset = r.ReadUInt32();
        Content = string.Empty;
      }

      public ChunkType Type { get; }
      public ushort Version { get; }
      public uint Id { get; }
      public int Size { get; }
      public uint Offset { get; }
      public string Content { get; private set; }

      internal void Read(BinaryReader r)
      {
        switch (Type)
        {
          case ChunkType.Unknow:
            Debug.WriteLine("ChCr: Unknown chunk. Ignoring.");
            break;

          case ChunkType.CryXml:

            r.BaseStream.Seek(Offset, SeekOrigin.Begin);
            var x = CryXmlSerializer.ReadStream(r.BaseStream);
            StringBuilder text = new StringBuilder();
            using (TextWriter writer = new StringWriter(text))
            {
              x.Save(writer);
            }
            Content = text.ToString();
            break;

          case ChunkType.Json:
            r.BaseStream.Seek(Offset, SeekOrigin.Begin);
            Content = Encoding.UTF8.GetString(r.ReadBytes(Size));
            break;

          default:
            break;
        }
      }
    }

    private enum ChunkType : ushort
    {
      Unknow = 2,
      CryXml = 4,
      Json = 17
    }
  }
}
