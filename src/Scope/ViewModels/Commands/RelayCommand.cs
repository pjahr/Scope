using Scope.Utils;
using System;
using System.Windows.Input;

namespace Scope.ViewModels.Commands
{
  internal class RelayCommand : ICommand
  {
    private readonly Action _action;
    private bool _isEnabled = true;

    public bool IsEnabled
    {
      get => _isEnabled; internal set
      {
        if (_isEnabled==value)
        {
          return;
        }
        _isEnabled = value;
        CanExecuteChanged.Raise(this);
      }
    }

    public event EventHandler CanExecuteChanged;

    public RelayCommand(Action action)
    {
      _action = action;
    }

    public bool CanExecute(object parameter)
    {
      return _isEnabled;
    }

    public void Execute(object parameter)
    {
      _action();
    }
  }

  internal class RelayCommand<T> : ICommand
  {
    private readonly Action<T> _action;

    public event EventHandler CanExecuteChanged;

    public RelayCommand(Action<T> action)
    {
      _action = action;
    }

    public bool CanExecute(object parameter)
    {
      return true;
    }

    public void Execute(object parameter)
    {
      _action((T) parameter);
    }
  }
}
