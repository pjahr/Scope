using Scope.Interfaces;
using System.IO;
using System.Text;

namespace Scope.Tests.Models
{
  internal class FileFake:IFile
  {
    private byte[] _bytes;

    public string Name { get; set; }
    public string Path { get; set; }
    public long BytesCompressed { get; set; }
    public long BytesUncompressed { get; set; }

    public string Text
    {
      get { return Encoding.UTF8.GetString(_bytes); }
      set { _bytes = Encoding.UTF8.GetBytes(value); }
    }

    public Stream Read()
    {
      return new MemoryStream(_bytes);
    }
  }
}