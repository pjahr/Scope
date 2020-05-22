using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using Scope.Models.Interfaces;
using Scope.Utils;
using Scope.ViewModels.Commands;

namespace Scope.ViewModels
{
  [Export]
  internal class PinnedItemsViewModel : INotifyPropertyChanged, IDisposable
  {
    private readonly ICurrentItem _currentItem;
    private readonly IPinnedItems _pinnedItems;
    private readonly IOutputDirectory _outputDirectory;

    public PinnedItemsViewModel(ICurrentItem currentItem,
                                IPinnedItems pinnedItems,
                                IOutputDirectory outputDirectory)
    {
      _currentItem = currentItem;
      _pinnedItems = pinnedItems;
      _outputDirectory = outputDirectory;

      Items = new ObservableCollection<object>();
      ChooseOutputDirectoryCommand = new RelayCommand(ChooseOutputDirectory);

      _pinnedItems.Changed += Update;
      _outputDirectory.Changed += RaiseOutputDirectoryChanged;

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

      var directories = _pinnedItems.Directories
                                    .OrderBy(d => d.Name)
                                    .Select(d => new PinnedDirectoryViewModel(d,
                                                                              _currentItem,
                                                                              _pinnedItems));
      foreach (var item in directories)
      {
        Items.Add(item);
      }

      var files = _pinnedItems.Files
                              .OrderBy(d => d.Name)
                              .Select(f => new PinnedFileViewModel(f,
                                                                  _currentItem,
                                                                  _pinnedItems));
      foreach (var item in files)
      {
        Items.Add(item);
      }
    }

    public void Dispose()
    {
      _pinnedItems.Changed -= Update;
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
