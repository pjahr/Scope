using System.Diagnostics;
using Scope.ViewModels.Commands;

namespace Scope.ViewModels
{
  internal class OpenRedditMenuItemViewModel : MenuItemBase
  {
    public OpenRedditMenuItemViewModel()
    {
      Label = "Star Citizen Modding Reddit";
      Command = new RelayCommand(OpenWebsite);
    }

    private void OpenWebsite()
    {
      Process.Start("https://www.reddit.com/r/StarCitizenModding");
    }
  }
}