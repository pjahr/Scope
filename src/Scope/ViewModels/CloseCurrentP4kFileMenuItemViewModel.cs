using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Nito.Mvvm;
using Scope.Models.Interfaces;

namespace Scope.ViewModels
{
  [Export]
  internal class CloseCurrentP4kFileMenuItemViewModel : MenuItemBase
  {
    private readonly ICurrentP4k _currentP4K;
    private readonly IAsyncCommand _command;

    public CloseCurrentP4kFileMenuItemViewModel(ICurrentP4k currentP4k)
    {
      _currentP4K = currentP4k;

      _command = new AsyncCommand(async () => { await CloseCurrentP4kFile(); });

      Command = _command;
      Label = "Close";
    }

    private async Task CloseCurrentP4kFile()
    {
      await _currentP4K.CloseAsync();
    }
  }
}
