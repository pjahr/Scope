using Microsoft.Win32;
using NAudio.Wave;
using Nito.Mvvm;
using Scope.FileViewer.WEM.Models;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Scope.FileViewer.WEM.ViewModels
{
  internal class WemFileViewModel
  {
    private readonly WemFileViewer _model;
    private bool _playing;

    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private readonly CancellationToken _ct;

    internal WemFileViewModel(WemFileViewer model)
    {
      _model = model;
      FileNameInternal = _model.FileName;
      PathInternal = "";//_model.PathInternal;
      LastModified = "";//$"{_model.LastModified}";

      Playing = false;
      _ct = _cts.Token;

      NumberOfBytesInternal = NotifyTask.Create(LoadDataAsync);
      OggFilePath = NotifyTask.Create(SaveAudioToTemporaryFile);
      PlayAudioCommand = new AsyncCommand(async () =>
      {
        try
        {
          await PlayAudioAsync();
        }
        catch (OperationCanceledException)
        {
        }

      });
      StopPlayingAudioCommand = new AsyncCommand(async () => await StopAudioAsync());
      SaveAudioToFileCommand = new AsyncCommand(async () => await SaveAudioToFileAsync());
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public string FileNameInternal { get; }
    public string PathInternal { get; }
    public string LastModified { get; }
    public NotifyTask<int> NumberOfBytesInternal { get; private set; }
    public NotifyTask<string> OggFilePath { get; private set; }
    public IAsyncCommand PlayAudioCommand { get; private set; }
    public IAsyncCommand StopPlayingAudioCommand { get; private set; }
    public IAsyncCommand SaveAudioToFileCommand { get; }

    public Visibility PlayVisible { get; private set; }
    public Visibility StopVisible { get; private set; }

    private bool Playing
    {
      get => _playing;
      set
      {
        _playing = value;
        PlayVisible = _playing ? Visibility.Hidden : Visibility.Visible;
        StopVisible = _playing ? Visibility.Visible : Visibility.Hidden;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayVisible)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StopVisible)));
      }
    }

    private async Task PlayAudioAsync()
    {
      using (var vorbisStream = new NAudio.Vorbis.VorbisWaveReader(OggFilePath.Result))
      using (var waveOut = new WaveOutEvent())
      {
        waveOut.Init(vorbisStream);
        waveOut.PlaybackStopped += Stop;
        Playing = true;
        await Task.Run(() =>
        {
          waveOut.Play();
          while (Playing)
          {
            if (_ct.IsCancellationRequested)
            {
              _ct.ThrowIfCancellationRequested();
            }
            Thread.Sleep(20);
          }
        }, _ct);
      }
    }

    private async Task StopAudioAsync()
    {
      await Task.Run(() => _cts.Cancel());
    }

    private async Task SaveAudioToFileAsync()
    {
      var saveFileDialog = new SaveFileDialog()
      {
        Filter = "WAV file (*.wav)|*.wav"
      };

      if (saveFileDialog.ShowDialog() == false)
      {
        return;
      }

      using (var vorbisStream = new NAudio.Vorbis.VorbisWaveReader(OggFilePath.Result))
      using (var waveOut = new WaveOutEvent())
      {
        await Task.Run(() => WaveFileWriter.CreateWaveFile(saveFileDialog.FileName, vorbisStream));
      }
    }

    private void Stop(object sender, StoppedEventArgs e)
    {
      Playing = false;
    }

    private async Task<string> SaveAudioToTemporaryFile()
    {
      return await _model.ConvertAsync();
    }

    private async Task<int> LoadDataAsync()
    {
      //MainWindow.SetStatus($"Loading data...");

      var n = await _model.GetNumberOfRawBytesAsync();

      //MainWindow.SetStatus($"Loaded data.");

      return n;
    }
  }
}
