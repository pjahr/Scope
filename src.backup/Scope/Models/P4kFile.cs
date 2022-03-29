using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Scope.Interfaces;
using Scope.Utils;
using Scope.Zip.Zip;

namespace Scope.Models
{
  internal class P4kFile : IFile
  {
    private readonly ZipEntry _zipEntry;
    private readonly ZipFile _zip;
    private readonly List<IDirectory> _directories = new List<IDirectory>();
    private readonly List<IFile> _files = new List<IFile>();

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

    public IReadOnlyCollection<IDirectory> Directories => _directories;
    public IReadOnlyCollection<IFile> Files => _files;
    public bool IsEmpty => _directories.Any() || _files.Any();

    internal void Add(IDirectory child)
    {
      _directories.Add(child);
    }

    internal void Add(IFile child)
    {
      _files.Add(child);
    }

    public Stream Read()
    {
      return _zip.GetInputStream(_zip.GetEntry(_zipEntry.Name));
    }
  }
}
