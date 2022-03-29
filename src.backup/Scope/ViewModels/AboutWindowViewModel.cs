using System;
using System.Windows.Input;
using Scope.Models.Interfaces;
using Scope.Utils;
using Scope.ViewModels.Commands;

namespace Scope.ViewModels
{
  internal class AboutWindowViewModel : IDialog
  {
    public AboutWindowViewModel()
    {
      CloseCommand = new RelayCommand(CloseRequested.Raise);
    }

    public ICommand CloseCommand { get; }

    public event Action CloseRequested;

    public void Dispose() { }
  }
}
