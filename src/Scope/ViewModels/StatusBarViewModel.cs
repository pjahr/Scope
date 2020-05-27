using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Scope.Models.Interfaces;
using Scope.Utils;
using Scope.ViewModels.Commands;

namespace Scope.ViewModels
{
  [Export]
  internal class StatusBarViewModel : INotifyPropertyChanged, IDisposable
  {
    private readonly IMessages _messages;
    private readonly IProgress _progress;

    private Paragraph _logText;

    public StatusBarViewModel(IMessages messages, IProgress progress)
    {
      _messages = messages;
      _progress = progress;

      _logText = new Paragraph { FontFamily = new FontFamily("Consolas"), FontSize = 10 };
      Messages = new FlowDocument(_logText);
      ToggleLogIsShownCommand = new RelayCommand(ToggleLogIsShown);
      FullLogIsShown = Visibility.Collapsed;

      DisplayProgress();

      _messages.MessageReceived += DisplayMessage;
      _progress.Changed += DisplayProgress;
    }

    private void DisplayProgress()
    {
      ProgressIndeterminate = _progress.Indetermined;
      ProgressValue = Convert.ToInt32(_progress.Value * 100);
      ProgressActive = _progress.InProgress
                         ? Visibility.Visible
                         : Visibility.Hidden;

      PropertyChanged.Raise(this, nameof(ProgressIndeterminate));
      PropertyChanged.Raise(this, nameof(ProgressValue));
      PropertyChanged.Raise(this, nameof(ProgressActive));
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public bool ProgressIndeterminate { get; private set; }
    public int ProgressValue { get; private set; }
    public Visibility ProgressActive { get; private set; }
    public string LastMessage { get; private set; }
    public FlowDocument Messages { get; }
    public Visibility FullLogIsShown { get; private set; }
    public ICommand ToggleLogIsShownCommand { get; private set; }

    public void Dispose()
    {
      _messages.MessageReceived -= DisplayMessage;
    }

    private void ToggleLogIsShown()
    {
      FullLogIsShown = FullLogIsShown == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
      PropertyChanged.Raise(this, nameof(FullLogIsShown));
    }

    private void DisplayMessage()
    {
      LastMessage = _messages.Items.Last()
                             .Text;

      _logText.Inlines.Add($"{LastMessage}\r\n");
      
      
      PropertyChanged.Raise(this, nameof(LastMessage));
    }

  }
}
