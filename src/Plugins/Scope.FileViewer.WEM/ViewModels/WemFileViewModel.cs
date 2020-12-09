using Microsoft.Win32;
using NAudio.Wave;
using Nito.Mvvm;
using Scope.FileViewer.WEM.Models;
using Scope.Interfaces;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Scope.Utils;

namespace Scope.FileViewer.WEM.ViewModels
{
  internal class WemFileViewModel : IFileViewer, INotifyPropertyChanged
  {
    private readonly WemFile _model;

    private CancellationTokenSource _cts = new CancellationTokenSource();
    private bool _playing = false;
    private string _oggFilePath;

    internal WemFileViewModel(WemFile model)
    {
      _model = model;

      Error = "";

      PlayAudioCommand = new AsyncCommand(PlayAudioAsync);
      StopPlayingAudioCommand = new AsyncCommand(StopAudioAsync);
      SaveAudioToFileCommand = new AsyncCommand(SaveAudioToFileAsync);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public AsyncCommand PlayAudioCommand { get; private set; }
    public AsyncCommand StopPlayingAudioCommand { get; private set; }
    public AsyncCommand SaveAudioToFileCommand { get; }

    public string Header { get; }
    public string Error { get; private set; }
    public Visibility PlayVisible { get; private set; }
    public Visibility StopVisible { get; private set; }

    private bool Playing
    {
      get => _playing;
      set
      {
        _playing = value;
        PlayVisible = _playing ? Visibility.Collapsed : Visibility.Visible;
        StopVisible = _playing ? Visibility.Visible : Visibility.Collapsed;

        PropertyChanged.Raise(this, nameof(PlayVisible));
        PropertyChanged.Raise(this, nameof(StopVisible));
      }
    }    

    private async Task PlayAudioAsync()
    {
      if (_oggFilePath==null)
      {
        await LoadDataAsync();
        _oggFilePath = await SaveAudioToTemporaryFile();
      }

      if (_oggFilePath=="")
      {
        return;
      }

      Playing = true;

      _cts = new CancellationTokenSource();

      using (var vorbisStream = new NAudio.Vorbis.VorbisWaveReader(_oggFilePath))
      using (var waveOut = new WaveOutEvent())
      {
        waveOut.Init(vorbisStream);
        waveOut.PlaybackStopped += Stop;
        var ct = _cts.Token;
        await Task.Run(() =>
        {
          waveOut.Play();
          while (Playing)
          {
            if (ct.IsCancellationRequested)
            {
              waveOut.Stop();
            }
            Thread.Sleep(20);
          }
        }, ct);
      }
    }

    private async Task StopAudioAsync()
    {
      await Task.Run(() => _cts.Cancel());
    }

    private void Stop(object sender, StoppedEventArgs e)
    {
      Playing = false;
    }

    private async Task<string> SaveAudioToTemporaryFile()
    {
      string pathToTmpfile = "";

      try
      {
        pathToTmpfile = await _model.ConvertAsync();
      }
      catch (Exception e)
      {
        Error = $"The file could not be converted.\r\n{e.Message}";
      }

      return pathToTmpfile;
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

      using (var vorbisStream = new NAudio.Vorbis.VorbisWaveReader(_oggFilePath))
      using (var waveOut = new WaveOutEvent())
      {
        await Task.Run(() => WaveFileWriter.CreateWaveFile(saveFileDialog.FileName, vorbisStream));
      }
    }

    private async Task<int> LoadDataAsync()
    {
      //MainWindow.SetStatus($"Loading data...");

      var n = await _model.GetNumberOfRawBytesAsync();

      //MainWindow.SetStatus($"Loaded data.");

      return n;
    }

    public void Dispose()
    {
      _cts.Cancel();
      _cts.Dispose();
    }
  }
}
