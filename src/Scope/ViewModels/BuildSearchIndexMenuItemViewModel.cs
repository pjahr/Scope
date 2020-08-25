using Scope.Models.Interfaces;
using Scope.ViewModels.Commands;
using System.ComponentModel.Composition;

namespace Scope.ViewModels
{
  [Export]
  internal class BuildSearchIndexMenuItemViewModel : MenuItemBase
  {
    private readonly ISearchIndex _searchIndex;
    private readonly ICurrentP4k _currentP4K;
    private readonly RelayCommand _command;

    public BuildSearchIndexMenuItemViewModel(ISearchIndex searchIndex, ICurrentP4k currentP4K)
    {
      _searchIndex = searchIndex;
      _currentP4K = currentP4K;
      _command = new RelayCommand(BuildSearchIndex);

      Command = _command;
      Label = "Build search index";

      _currentP4K.Changed += UpdateCommand;

      UpdateCommand();
    }

    private void UpdateCommand()
    {
      _command.IsEnabled = _currentP4K.IsInitialized;
    }

    private void BuildSearchIndex()
    {
      _searchIndex.BuildUp();
    }
  }
}
