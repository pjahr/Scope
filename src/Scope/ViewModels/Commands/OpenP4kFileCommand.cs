using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using Nito.Mvvm;
using Scope.Models.Interfaces;
using Scope.Utils;
using IFileSystem = System.IO.Abstractions.IFileSystem;

namespace Scope.ViewModels.Commands
{
  [Export]
  internal class OpenP4kFileCommand : ICommand
  {
    private readonly ICurrentP4k _currentP4K;
    private readonly IFileSystem _fileSystem;
    private readonly IMessages _messages;
    private readonly IProgress _progress;
    private readonly AsyncCommand _command;

    public OpenP4kFileCommand(ICurrentP4k currentP4k,
                              IFileSystem fileSystem,
                              IMessages messages,
                              IProgress progress)
    {
      _currentP4K = currentP4k;
      _fileSystem = fileSystem;
      _messages = messages;
      _progress = progress;

      _command = new AsyncCommand(async () => { await OpenP4kFile(); });
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
      return !_command.IsExecuting;
    }

    public void Execute(object parameter)
    {
      (_command as ICommand).Execute(parameter);
    }

    private async Task OpenP4kFile()
    {
      CanExecuteChanged.Raise(this);
      _command.Execution.PropertyChanged += UpdateCanExecuteWhenTaskIsComplete;

      var openFileDialog = new OpenFileDialog
                           {
                             DefaultExt = ".p4k", Filter = "Game archive (.p4k)|*.p4k"
                           };

      if (!openFileDialog.ShowDialog()
                         .Value)
      {
        return;
      }

      var file = _fileSystem.FileInfo.FromFileName(openFileDialog.FileName);

      _messages.Add($"Loading {openFileDialog.FileName}...");
      _progress.SetIndetermined();

      await _currentP4K.ChangeAsync(file);
    }

    private void UpdateCanExecuteWhenTaskIsComplete(object sender, PropertyChangedEventArgs _)
    {
      var task = (NotifyTask) sender;
      if (task.IsCompleted)
      {
        task.PropertyChanged -= UpdateCanExecuteWhenTaskIsComplete; // unhook this
        CanExecuteChanged
         .Raise(this); // raise event so that consumers (buttons) can reactivate
      }

      _messages.Add($"Loaded {_currentP4K.FileName}.");
      _progress.Stop();
    }
  }
}
