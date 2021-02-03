using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using static Scope.FileViewer.Text.Models.ByteOrderEnum;

namespace Scope.FileViewer.Text.Models
{
  public static class ChCrXmlSerializer
  {
    public static XmlDocument ReadFile(string file,
                                       ByteOrderEnum byteOrder = ByteOrderEnum.AutoDetect,
                                       bool writeLog = false)
    {
      return ReadStream(File.OpenRead(file), byteOrder, writeLog);
    }

    public static XmlDocument ReadStream(Stream stream,
                                         ByteOrderEnum byteOrder = ByteOrderEnum.AutoDetect,
                                         bool writeLog = false)
    {
      using (BinaryReader r = new BinaryReader(stream))
      {
        stream.Position = 0;

        var signature = r.ReadUInt32();
        var version = r.ReadUInt32();
        var numChunks = r.ReadUInt32();
        var chunkTableOffset = r.ReadUInt32();
        
        stream.Seek(chunkTableOffset, SeekOrigin.Current);        

        for (int i = 0; i < numChunks; i++)
        {
          var chunkType = r.ReadUInt16();
          var chunkVersion = r.ReadUInt16();
          var id = r.ReadUInt32();
          var size = r.ReadUInt32();
          var offset = r.ReadUInt32();
        }

        

        throw new NotImplementedException();
      }
    }

    private 
  }
}
