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
    private readonly IExtractP4kContent _extractP4KContent;

    public PinnedItemsViewModel(ICurrentItem currentItem,
                                IPinnedItems pinnedItems,
                                IOutputDirectory outputDirectory,
                                IExtractP4kContent extractP4KContent)
    {
      _currentItem = currentItem;
      _pinnedItems = pinnedItems;
      _outputDirectory = outputDirectory;
      _extractP4KContent = extractP4KContent;

      Items = new ObservableCollection<object>();
      ChooseOutputDirectoryCommand = new RelayCommand(ChooseOutputDirectory);
      ExtractCommand = new RelayCommand<object>(ExtractItem);

      _pinnedItems.Changed += Update;
      _outputDirectory.Changed += RaiseOutputDirectoryChanged;

      Update();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public string OutputDirectory => _outputDirectory.Path;
    public ICommand ChooseOutputDirectoryCommand { get; }
    public ICommand ExtractCommand { get; }

    public ObservableCollection<object> Items { get; private set; }
    public bool HasItems => Items.Any();

    public void Dispose()
    {
      _pinnedItems.Changed -= Update;
    }

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

      PropertyChanged.Raise(this, nameof(HasItems));
    }

    private void ExtractItem(object item)
    {
      switch (item)
      {
        case PinnedDirectoryViewModel directory:
          ExtractDirectory(directory);
          return;
        case PinnedFileViewModel file:
          ExtractFile(file);
          return;
        case null:
          ExtractAll();
          return;
      }
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

    private void ExtractAll()
    {
      foreach (var directory in _pinnedItems.Directories)
      {
        _extractP4KContent.Extract(directory);
      }
      foreach (var file in _pinnedItems.Files)
      {
        _extractP4KContent.Extract(file);
      }
    }

    private void ExtractFile(PinnedFileViewModel file)
    {
      _extractP4KContent.Extract(file.Model);
    }

    private void ExtractDirectory(PinnedDirectoryViewModel directory)
    {
      _extractP4KContent.Extract(directory.Model);
    }

    private void RaiseOutputDirectoryChanged()
    {
      PropertyChanged.Raise(this, nameof(OutputDirectory));
    }
  }
}
