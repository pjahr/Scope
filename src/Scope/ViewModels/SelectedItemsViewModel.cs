using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Scope.Models.Interfaces;

namespace Scope.ViewModels
{
  [Export]
  internal class SelectedItemsViewModel : INotifyPropertyChanged, IDisposable
  {
    private readonly ICurrentItem _currentItem;
    private readonly ISelectedItems _selectedItems;

    public SelectedItemsViewModel(ICurrentItem currentItem, ISelectedItems selectedItems)
    {
      _currentItem = currentItem;
      _selectedItems = selectedItems;

      Items = new ObservableCollection<object>();
      _selectedItems.Changed += Update;

      Update();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<object> Items { get; private set; }

    private void Update()
    {
      Items.Clear();
      foreach (var item in _selectedItems.Directories.OrderBy(d => d.Name)
                                         .Select(f => new SelectedDirectoryViewModel(f,
                                                                                     _currentItem,
                                                                                     _selectedItems))
      )
      {
        Items.Add(item);
      }

      foreach (var item in _selectedItems.Files.OrderBy(d => d.Name)
                                         .Select(f => new SelectedFileViewModel(f,
                                                                                _currentItem,
                                                                                _selectedItems)))
      {
        Items.Add(item);
      }
    }

    public void Dispose()
    {
      _selectedItems.Changed -= Update;
    }
  }
}
