using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Scope.Models.Interfaces;
using Scope.Utils;
using Scope.ViewModels.Commands;

namespace Scope.ViewModels
{
  [Export]
  internal class SelectedItemsViewModel : INotifyPropertyChanged, IDisposable
  {
    private readonly ICurrentItem _currentItem;
    private readonly ISelectedItems _selectedItems;
    private readonly IOutputDirectory _outputDirectory;

    public SelectedItemsViewModel(ICurrentItem currentItem, ISelectedItems selectedItems, IOutputDirectory outputDirectory)
    {
      _currentItem = currentItem;
      _selectedItems = selectedItems;
      _outputDirectory = outputDirectory;
      Items = new ObservableCollection<object>();
      _selectedItems.Changed += Update;
      _outputDirectory.Changed += RaiseOutputDirectoryChanged;

      ChooseOutputDirectoryCommand = new RelayCommand(ChooseOutputDirectory);

      Update();
    }

    private void RaiseOutputDirectoryChanged()
    {
      PropertyChanged.Raise(this, nameof(OutputDirectory));
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public string OutputDirectory => _outputDirectory.Path;
    public ICommand ChooseOutputDirectoryCommand { get; }

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

    private void ChooseOutputDirectory()
    {
      var chooseDirectoryDialog = new CommonOpenFileDialog { IsFolderPicker = true };

      var result = chooseDirectoryDialog.ShowDialog();
      if (result != CommonFileDialogResult.Ok)
      {
        return;
      }
      _outputDirectory.Path = chooseDirectoryDialog.FileName;
    }
  }
}
