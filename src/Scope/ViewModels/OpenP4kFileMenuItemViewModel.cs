using System.ComponentModel.Composition;
using Scope.ViewModels.Commands;

namespace Scope.ViewModels
{
  [Export]
  internal class OpenP4kFileMenuItemViewModel : MenuItemBase
  {
    public OpenP4kFileMenuItemViewModel(OpenP4kFileCommand command)
    {
      Command = command;
      Label = "Open...";
    }

  }
}