using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Input;
using Scope.Models.Interfaces;
using Scope.Utils;
using Scope.ViewModels.Commands;

namespace Scope.ViewModels
{
  [Export]
  internal class MainWindowViewModel : INotifyPropertyChanged, IDisposable
  {
    private readonly ICurrentP4k _currentP4K;
    private readonly OpenP4kFileCommand _openP4KFileCommand;

    public MainWindowViewModel(ICurrentP4k currentP4k,
                               MainMenuViewModel mainMenu,
                               CurrentP4kFileSystemViewModel currentP4KFileSystem,
                               PinnedItemsViewModel selectedItems,
                               StatusBarViewModel statusBar,
                               FileViewerCollectionViewModel fileViewers,
                               OpenP4kFileCommand openP4KFileCommand)
    {
      _currentP4K = currentP4k;
      _openP4KFileCommand = openP4KFileCommand;

      MainMenu = mainMenu;
      CurrentP4kFileSystem = currentP4KFileSystem;
      StatusBar = statusBar;
      FileViewers = fileViewers;
      SelectedItems = selectedItems;

      Title = "Scope";

      _currentP4K.Changed += UpdateTitle;
    }

    public string Title { get; private set; }
    public MainMenuViewModel MainMenu { get; }
    public CurrentP4kFileSystemViewModel CurrentP4kFileSystem { get; }
    public StatusBarViewModel StatusBar { get; }
    public FileViewerCollectionViewModel FileViewers { get; }
    public PinnedItemsViewModel SelectedItems { get; }

    public ICommand OpenP4kFileCommand => _openP4KFileCommand;

    public event PropertyChangedEventHandler PropertyChanged;

    public void Dispose()
    {
      _currentP4K.Changed -= UpdateTitle;
    }

    private void UpdateTitle()
    {
      // 'Scope - P:\ath\to\game\Data.p4k'
      var spacer = _currentP4K.FileSystem != null
                     ? " - "
                     : "";
      Title = $"Scope{spacer}{_currentP4K.FileName}";

      PropertyChanged.Raise(this, nameof(Title));
    }
  }
}
