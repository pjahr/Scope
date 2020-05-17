using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Scope.Interfaces;
using Scope.Models.Interfaces;
using Scope.Utils;

namespace Scope.ViewModels
{
  [Export]
  internal class FileViewerCollectionViewModel : INotifyPropertyChanged
  {
    private readonly ICurrentItem _currentItem;
    private readonly IEnumerable<IFileViewerFactory> _fileViewerFactories;

    public event PropertyChangedEventHandler PropertyChanged;

    public FileViewerCollectionViewModel(ICurrentItem currentItem,
                                         IEnumerable<IFileViewerFactory> fileViewerFactories = null)
    {
      _currentItem = currentItem;
      _fileViewerFactories = fileViewerFactories;

      Items = new ObservableCollection<FileItemViewModel>();

      _currentItem.Changed += UpdateFileViewers;
    }

    private void UpdateFileViewers()
    {
      var current = Items.ToArray();

      Items.Clear();

      foreach (var item in current)
      {
        item.Dispose();
      }

      if (_currentItem.CurrentFile == null)
      {
        return;
      }

      var factory = _fileViewerFactories
                     .FirstOrDefault(f => f.CanHandle(_currentItem.CurrentFile));

      if (factory == null)
      {
        factory = new NoFileViewerViewModelFactory();
      }

      Items.Add(new FileItemViewModel(_currentItem.CurrentFile, factory));
      Active = Items.First();
      PropertyChanged.Raise(this, nameof(Active));
    }

    public ObservableCollection<FileItemViewModel> Items { get; }
    public FileItemViewModel Active { get; set; }
  }
}