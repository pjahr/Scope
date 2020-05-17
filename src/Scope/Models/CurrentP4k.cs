using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Scope.Models.Interfaces;
using Scope.Utils;
using Scope.Zip.Zip;

namespace Scope.Models
{
  [Export]
  internal class CurrentP4k : ICurrentP4k
  {
    private readonly byte[] _key = new byte[] { 94, 122, 32, 2, 48, 46, 235, 26, 59, 182, 23, 195, 15, 222, 30, 71 };
    private ZipFile _p4k;

    public IFileSystem FileSystem { get; private set; }
    public string FileName { get; private set; } = "";

    public event Action Changed;

    public void Dispose()
    {
      DisposeCurrentP4k();
    }

    public Task<OpenP4kFileResult> ChangeAsync(System.IO.Abstractions.IFileInfo p4kFile)
    {
      DisposeCurrentP4k();

      if (!p4kFile.Exists)
      {
        var failure = $"File '{p4kFile.FullName}' was not found.";
        return Task.FromResult(new OpenP4kFileResult(failure));
      }

      return Task.Run(() => OpenP4kFile(p4kFile));
    }

    public Task CloseAsync()
    {
      return Task.Run(DisposeCurrentP4k);
    }

    private OpenP4kFileResult OpenP4kFile(System.IO.Abstractions.IFileInfo p4kFile)
    {
      try
      {
        _p4k = new ZipFile(p4kFile.FullName) { Key = _key };
        //_p4k.KeysRequired += ProvideKey;

        FileSystem = new GenerateFileSystem().Generate(_p4k);
        FileName = p4kFile.FullName;

      }
      catch (Exception ex)
      {
        DisposeCurrentP4k();
        return new OpenP4kFileResult(ex.Message);
      }

      Changed.Raise();

      return new OpenP4kFileResult();
    }

    private void ProvideKey(object sender, KeysRequiredEventArgs e)
    {
      e.Key = _key;
    }

    private void DisposeCurrentP4k()
    {
      if (_p4k != null)
      {
        _p4k.KeysRequired -= ProvideKey;
        _p4k.Close();
        _p4k = null;
      }
      FileSystem = null;
      FileName = "";

      Changed.Raise();
    }
  }
}
