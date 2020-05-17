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

      Name = _zipEntry.Name.GetFileName();
      Path = _zipEntry.Name;
    }
    public string Name { get; }
    public string Path { get; }    

    public Stream Read()
    {
      return _zip.GetInputStream(_zip.GetEntry(_zipEntry.Name));
    }
  }
}
