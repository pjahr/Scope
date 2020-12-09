using Scope.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using WEMSharp;

namespace Scope.FileViewer.WEM.Models
{
  internal class WemFile : IFileViewer
  {
    private readonly IFile _file;
    private readonly IMessageQueue _messageQueue;
    private byte[] _rawData;

    public WemFile(IFile file, IMessageQueue messageQueue)
    {
      _file = file;
      _messageQueue = messageQueue;
    }

    public string Header => _file.Name;

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
      if (_rawData == null)
      {
        _rawData = await Task.Run(() => GetRawBytes());
      }

      var tmpDirectory = Path.GetTempPath();

      var fileName = $"Scope_tmpWEM_{_file.Path.Replace('/','.')}";
      var wemFileName = $"{fileName}.wem";
      var oggFileName = $"{fileName}.ogg";

      var wemFilePath = Path.Combine(tmpDirectory, wemFileName);
      var oggFilePath = Path.Combine(tmpDirectory, oggFileName);

      if (File.Exists(wemFilePath))
      {
        _messageQueue.Add($"Found {wemFilePath}");
      }
      else
      {
        File.WriteAllBytes(wemFilePath, _rawData);
        _messageQueue.Add($"Wrote {wemFilePath}");
      }

      if (File.Exists(oggFilePath))
      {
        _messageQueue.Add($"Found {wemFilePath}");
        return oggFilePath;
      }

      WEMFile wemFile;
      try
      {
        wemFile = new WEMFile(wemFilePath, WEMForcePacketFormat.NoForcePacketFormat);
      }
      catch (Exception)
      {
        throw;
      }

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

      _messageQueue.Add($"Wrote {oggFilePath}");

      return oggFilePath;
    }

    public void Dispose()
    {      
    }
  }
}
