using System.ComponentModel;
using System.ComponentModel.Composition;
using Scope.Models.Interfaces;
using Scope.Utils;

namespace Scope.ViewModels
{
  [Export]
  internal class CurrentP4kFileSystemViewModel : INotifyPropertyChanged
  {
    private readonly ICurrentP4k _currentP4K;
    private readonly ICurrentItem _currentFile;
    private readonly ISelectedItems _selectedItems;

    public CurrentP4kFileSystemViewModel(ICurrentP4k currentP4k,
                                         ICurrentItem currentFile,
                                         ISelectedItems selectedItems)
    {
      _currentP4K = currentP4k;
      _currentFile = currentFile;
      _selectedItems = selectedItems;
      Initialize();

      _currentP4K.Changed += Initialize;
    }

    private void Initialize()
    {
      FileSystem = _currentP4K.FileSystem != null
                      ? new P4kFileSystemViewModel(_currentP4K.FileSystem, 
                                                   _currentFile,
                                                   _selectedItems)
                      : null;

      PropertyChanged.Raise(this, nameof(FileSystem));
    }

    public P4kFileSystemViewModel FileSystem { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
