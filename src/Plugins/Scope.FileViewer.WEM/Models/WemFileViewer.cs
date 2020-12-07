using Scope.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using WEMSharp;

namespace Scope.FileViewer.WEM.Models
{
  internal class WemFileViewer : IFileViewer
  {
    private readonly IFile _file;
    private byte[] _rawData;

    public WemFileViewer(IFile file)
    {
      _file = file;
    }

    public string FileName => _file.Name;
    //public string PathInternal => _file.FullPath;
    //public DateTime LastModified => _file.LastModifiedDate;

    public string Header { get; }

    public async Task<int> GetNumberOfRawBytesAsync()
    {
      if (_rawData == null)
      {
        _rawData = await Task.Run(() => GetRawBytes());
      }
      return _rawData.Length;
    }

    private byte[] GetRawBytes()
    {
      using (var stream = _file.Read())
      using (var memoryStream = new MemoryStream())
      {
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
      }
    }

    public async Task<string> ConvertAsync()
    {
      var tmpDirectory = Path.GetTempPath();

      var fileName = $"Crucible_tmpWEM_{Guid.NewGuid()}";
      var wemFileName = $"{fileName}.wem";
      var oggFileName = $"{fileName}.ogg";

      var wemFilePath = Path.Combine(tmpDirectory, wemFileName);
      var oggFilePath = Path.Combine(tmpDirectory, oggFileName);

      File.WriteAllBytes(wemFilePath, _rawData);

      var wemFile = new WEMFile(wemFilePath, WEMForcePacketFormat.NoForcePacketFormat);
      //MainWindow.SetStatus($"Wrote {wemFilePath}");

      var codebookPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Plugins\Resources\packed_codebooks_aoTuV_603.bin");
      try
      {
        wemFile.GenerateOGG(oggFilePath, codebookPath, false, false);
      }
      catch (Exception e)
      {
        // TODO: fail gacefully
        throw new InvalidOperationException($"Failed to write to {oggFilePath}.\\nCodebook path: {codebookPath}", e);
      }

      var revorbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Plugins\Resources\revorb.exe");

      var exitCode = await External.RunProcessAsync(revorbPath, oggFilePath);

      //MainWindow.SetStatus($"Wrote {oggFilePath}");

      return oggFilePath;
    }

    public void Dispose()
    {
      
    }
  }
}
