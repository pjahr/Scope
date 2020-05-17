using System.Collections.Generic;

namespace Scope.ViewModels
{
  internal class RootMenuItemViewModel
  {
    public RootMenuItemViewModel(string label, params MenuItemBase[] items)
    {
      Label = label;
      Items = new List<MenuItemBase>(items);
    }

    public string Label { get; }
    public IReadOnlyList<MenuItemBase> Items { get; }
  }
}