using Nito.Mvvm;
using Scope.Models.Interfaces;
using Scope.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Scope.ViewModels.Commands
{
  [Export]
  internal class InitiateSearchCommand : ICommand
  {
    private readonly IProgress _progress;
    private readonly AsyncCommand _command;
    private readonly ISearch _searchIndex;
    private readonly bool _isRunning;

    public InitiateSearchCommand(ISearch searchIndex)
    {
      _searchIndex = searchIndex;

      _command = new AsyncCommand(async (t) => { await InitiateSearch(t); });
    }

    private async Task InitiateSearch(object searchText)
    {
      if (_isRunning)
      {
        CancelPreviousSearch();
      }

      var terms = ((string)searchText).Split(' ');
      
      //await _searchIndex.FindAsync(terms);
    }

    private void CancelPreviousSearch()
    {
      // cts
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
      throw new NotImplementedException();
    }

    public void Execute(object parameter)
    {
      (_command as ICommand).Execute(parameter);
    }
  }
}
