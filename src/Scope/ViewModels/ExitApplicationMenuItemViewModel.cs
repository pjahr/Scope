using System.Windows;
using Scope.ViewModels.Commands;

namespace Scope.ViewModels
{
  internal class ExitApplicationMenuItemViewModel : MenuItemBase
  {
    public ExitApplicationMenuItemViewModel()
    {
      Label = "Exit";
      Shortcut = "Alt + F4";
      Command = new RelayCommand(ShutdownApplication);
    }

    private void ShutdownApplication()
    {
      Application.Current.Shutdown();
    }
  }
}