using Scope.Models.Interfaces;
using Scope.ViewModels.Commands;
using System.ComponentModel.Composition;

namespace Scope.ViewModels
{
  [Export]
  internal class BuildSearchIndexMenuItemViewModel : MenuItemBase
  {
    private readonly ISearch _searchIndex;
    private readonly ICurrentP4k _currentP4K;
    private readonly RelayCommand _command;

    public BuildSearchIndexMenuItemViewModel(ISearch searchIndex, ICurrentP4k currentP4K)
    {
      _searchIndex = searchIndex;
      _currentP4K = currentP4K;
      _command = new RelayCommand(BuildSearchIndex);

      Command = _command;

      _currentP4K.Changed += UpdateCommand;

      UpdateCommand();
    }

    private void UpdateCommand()
    {
      var isEnabled = _currentP4K.IsInitialized;
      _command.IsEnabled = isEnabled;

      Label = isEnabled
              ?"Build search index"
              : "Build search index (load P4K first)";

      Tooltip = isEnabled
                ? "Scans all known text files and generates an index of all containing words to increase search speed."
                : "Please load a P4K file first.";
    }

    private void BuildSearchIndex()
    {
      _searchIndex.BuildUp();
    }
  }
}
