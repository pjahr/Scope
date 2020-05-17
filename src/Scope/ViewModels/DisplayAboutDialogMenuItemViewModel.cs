using System.ComponentModel.Composition;
using Scope.Models.Interfaces;
using Scope.ViewModels.Commands;

namespace Scope.ViewModels
{
  [Export]
  internal class DisplayAboutDialogMenuItemViewModel : MenuItemBase
  {
    private readonly IDialogs _dialogs;

    public DisplayAboutDialogMenuItemViewModel(IDialogs dialogs)
    {
      _dialogs = dialogs;

      Label = "About";
      Command = new RelayCommand(DisplayAboutDialog);
    }

    private void DisplayAboutDialog()
    {
      _dialogs.Show(new AboutWindowViewModel());
    }
  }
}