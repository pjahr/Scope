using System.Diagnostics;
using Scope.Utils;

namespace Scope.ViewModels
{
  internal class OpenDiscordMenuItemViewModel : MenuItemBase
  {
    public OpenDiscordMenuItemViewModel()
    {
      Label = "Star Citizen Modding Discord";
      Command = new RelayCommand(OpenWebsite);
    }

    private void OpenWebsite()
    {
      Process.Start("https://discord.gg/Mk3hz5b");
    }
  }
}
