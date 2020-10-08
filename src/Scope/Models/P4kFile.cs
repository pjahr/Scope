using System;
using System.IO;
using Scope.Interfaces;
using Scope.Utils;
using Scope.Zip.Zip;

namespace Scope.Models
{
  internal class P4kFile : IFile
  {
    private readonly ZipEntry _zipEntry;
    private readonly ZipFile _zip;

    public P4kFile(ZipEntry zipEntry, ZipFile zip)
    {
      _zipEntry = zipEntry;
      _zip = zip;

      Index = Convert.ToInt32(_zipEntry.ZipFileIndex);
      Name = _zipEntry.Name.GetFileName();
      Path = _zipEntry.Name;
      BytesCompressed = _zipEntry.CompressedSize;
      BytesUncompressed = _zipEntry.Size;
    }

    public int Index { get; }
    public string Name { get; }
    public string Path { get; }
    public long BytesCompressed { get; }
    public long BytesUncompressed { get; }

    public Stream Read()
    {
      return _zip.GetInputStream(_zip.GetEntry(_zipEntry.Name));
    }
  }
}
