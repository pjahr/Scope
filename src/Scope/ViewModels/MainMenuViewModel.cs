﻿using System.ComponentModel.Composition;

namespace Scope.ViewModels
{
  [Export]
  internal class MainMenuViewModel
  {
    public MainMenuViewModel(OpenP4kFileMenuItemViewModel openP4kFile,
                             CloseCurrentP4kFileMenuItemViewModel closeP4kFile,
                             DisplayAboutDialogMenuItemViewModel displayAboutDialog,
                             BuildSearchIndexMenuItemViewModel buildSearchIndex)
    {
      OpenP4kFile = openP4kFile;
      CloseP4kFile = closeP4kFile;
      DisplayAboutDialog = displayAboutDialog;
      BuildSearchIndex = buildSearchIndex;
    }

    // File
    public MenuItemBase OpenP4kFile { get; }
    public MenuItemBase CloseP4kFile { get; }
    public MenuItemBase ExitApplication { get; } = new ExitApplicationMenuItemViewModel();
    public MenuItemBase BuildSearchIndex { get; }

    // Help
    public MenuItemBase OpenReddit { get; } = new OpenRedditMenuItemViewModel();
    public MenuItemBase OpenDiscord { get; } = new OpenDiscordMenuItemViewModel();
    public MenuItemBase DisplayAboutDialog { get; }
  }
}
