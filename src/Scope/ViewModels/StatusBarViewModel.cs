using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using Scope.Models.Interfaces;
using Scope.Utils;

namespace Scope.ViewModels
{
  [Export]
  internal class StatusBarViewModel : INotifyPropertyChanged, IDisposable
  {
    private readonly IMessages _messages;
    private readonly IProgress _progress;

    public StatusBarViewModel(IMessages messages, IProgress progress)
    {
      _messages = messages;
      _progress = progress;

      DisplayProgress();

      _messages.MessageReceived += DisplayMessage;
      _progress.Changed += DisplayProgress;
    }

    private void DisplayProgress()
    {
      ProgressIndeterminate = _progress.Indetermined;
      ProgressValue = Convert.ToInt32(_progress.Value*100);
      ProgressActive = _progress.InProgress?Visibility.Visible:Visibility.Hidden;

      PropertyChanged.Raise(this, nameof(ProgressIndeterminate));
      PropertyChanged.Raise(this, nameof(ProgressValue));
      PropertyChanged.Raise(this, nameof(ProgressActive));
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public void Dispose()
    {
      _messages.MessageReceived -= DisplayMessage;
    }

    private void DisplayMessage()
    {
      LastMessage = _messages.Items.Last().Text;
      PropertyChanged.Raise(this, nameof(LastMessage));
    }

    public string LastMessage { get; private set; }
    public bool ProgressIndeterminate { get; private set; }
    public int ProgressValue { get; private set; }
    public Visibility ProgressActive { get; private set; }
  }
}
