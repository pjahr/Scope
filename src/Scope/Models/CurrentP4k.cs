using System;
using System.ComponentModel.Composition;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Scope.Models.Interfaces;
using Scope.Utils;
using Scope.Zip.Zip;
using IFileSystem = Scope.Models.Interfaces.IFileSystem;

namespace Scope.Models
{
  [Export]
  internal class CurrentP4k : ICurrentP4k
  {
    private readonly byte[] _key =
    {
      94, 122, 32, 2, 48, 46, 235, 26, 59, 182, 23, 195, 15, 222, 30, 71
    };
    private readonly IOutputDirectory _outputDirectory;
    private ZipFile _p4k;

    public CurrentP4k(IOutputDirectory outputDirectory)
    {
      _outputDirectory = outputDirectory;
    }

    public IFileSystem FileSystem { get; private set; }
    public string FileName { get; private set; } = "";

    public event Action Changed;

    public void Dispose()
    {
      DisposeCurrentP4k();
    }

    public Task<OpenP4kFileResult> ChangeAsync(IFileInfo p4kFile)
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

    private OpenP4kFileResult OpenP4kFile(IFileInfo p4kFile)
    {
      try
      {
        _p4k = new ZipFile(p4kFile.FullName) {Key = _key};
        
        FileSystem = new GenerateFileSystem().Generate(_p4k);
        FileName = p4kFile.FullName;
      }
      catch (Exception ex)
      {
        DisposeCurrentP4k();
        return new OpenP4kFileResult(ex.Message);
      }

      if (_outputDirectory.Path==null)
      {
        _outputDirectory.Path = p4kFile.DirectoryName;
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
