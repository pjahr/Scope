using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Nito.Mvvm;
using Scope.Interfaces;
using Scope.Models.Interfaces;
using Scope.Utils;
using Scope.ViewModels.Factories;

namespace Scope.ViewModels
{
  internal class FileTreeNodeViewModel : ITreeNodeViewModel
  {
    private static readonly LoadingTreeNodeViewModel Loading = new LoadingTreeNodeViewModel();

    private readonly ObservableCollection<ITreeNodeViewModel> _children;
    private readonly string _path;
    private readonly bool _hasChildren;
    private readonly ISearch _search;
    private readonly ISearchOptions _searchOptions;
    private readonly IUiDispatch _uiDispatch;
    private readonly IFileSystemTreeNodeViewModelFactory _fileSystemTreeNodeViewModelFactory;
    private readonly IFileSubStructureProvider[] _fileSubStructureProviders;

    private string _name;
    private bool _isExpanded;
    private bool _isSelected;

    public FileTreeNodeViewModel(IFile file,
                                 ISearch search,
                                 ISearchOptions searchOptions,
                                 IUiDispatch uiDispatch,
                                 IFileSubStructureProvider[] fileSubStructureProviders,
                                 IFileSystemTreeNodeViewModelFactory fileSystemTreeNodeViewModelFactory)
    {
      Model = file;

      _fileSubStructureProviders = fileSubStructureProviders
                                    .Where(f => f.ApplicableFileExtension
                                                == System.IO.Path.GetExtension(file.Name))
                                    .ToArray();

      _hasChildren = fileSubStructureProviders.Any();
      _path = file.Path;
      _name = file.Name;
      _search = search;
      _searchOptions = searchOptions;
      _uiDispatch = uiDispatch;
      _fileSystemTreeNodeViewModelFactory = fileSystemTreeNodeViewModelFactory;

      var compressed = file.BytesCompressed.ToFileSize()
                           .Split(' ');
      CompressedSizeValue = compressed[0];
      CompressedSizeUnit = compressed[1];

      var uncompressed = file.BytesUncompressed.ToFileSize()
                             .Split(' ');

      UncompressedSizeValue = uncompressed[0];
      UncompressedSizeUnit = uncompressed[1];


      _search.Finished += HighlightSearchTerm;
      _search.ResultsCleared += ResetName;

      ExpandCommand = new AsyncCommand(LoadChildrenAsync);

      HighlightSearchTerm();
    }

    public void Dispose()
    {
      Console.WriteLine($"Disposing node: {Model}");
      _search.Finished += HighlightSearchTerm;
      _search.ResultsCleared += ResetName;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public ICommand ExpandCommand { get; }
    public string Path => _path;
    public ObservableCollection<ITreeNodeViewModel> Children => _children;
    public bool HasChildren => _hasChildren;

    public IFile Model { get; }
    public string CompressedSizeValue { get; }
    public string CompressedSizeUnit { get; }
    public string UncompressedSizeValue { get; }
    public string UncompressedSizeUnit { get; }


    public bool IsExpanded
    {
      get => _isExpanded;
      set
      {
        if (value == _isExpanded)
        {
          return;
        }

        _isExpanded = value;
        PropertyChanged.Raise(this, nameof(IsExpanded));
      }
    }


    public bool IsSelected
    {
      get => _isSelected;
      set
      {
        if (value == _isSelected)
        {
          return;
        }

        _isSelected = value;
        PropertyChanged.Raise(this, nameof(IsSelected));
      }
    }

    public string Name
    {
      get => _name;
      set
      {
        if (value == _name)
        {
          return;
        }

        _name = value;
        PropertyChanged.Raise(this, nameof(Name));
      }
    }

    public void SetExpand(bool isExpanded)
    {
      IsExpanded = isExpanded;
    }

    public void SetSelect(bool isSelected)
    {
      IsSelected = isSelected;
    }

    private void ResetChildren()
    {
      ExpandCommand.Execute(null);
    }

    private async Task LoadChildrenAsync()
    {
      if (!HasChildren)
      {
        return;
      }
      Children.Clear();
      Children.Add(Loading);

      var directories = new List<DirectoryTreeNodeViewModel>();
      var files = new List<FileTreeNodeViewModel>();

      try
      {
        foreach (var fssp in _fileSubStructureProviders)
        {
          var root = await fssp.GetAsDirectoryAsync(Model, new Progress<ProgressReport>());

          //_search.FileResults.Any()
          //  ? files.Where(f => _search.ResultIds.Contains(f.Index))
          //  : files

          foreach (var directory in root.Directories)
          {
            directories.Add(_fileSystemTreeNodeViewModelFactory.Create(directory));
          }

          foreach (var file in root.Files)
          {
            files.Add(_fileSystemTreeNodeViewModelFactory.Create(file));
          }

        }
      }
      catch (Exception e)
      {
        Console.WriteLine($"Failed loading children of {Name}.\r\n{e.Message}");
      }
      finally
      {
        Children.Remove(Loading);
        foreach (var directory in directories.OrderBy(d => d.Name))
        {
          Children.Add(directory);
        }
        foreach (var file in files.OrderBy(f => f.Name))
        {
          Children.Add(file);
        }
      }
    }

    private void FilterContent()
    {
      switch (_searchOptions.Mode)
      {
        case SearchMode.FileName:
          RemoveContentsThatDoNotMatchSearchTerm();
          return;
        case SearchMode.FileContent:
          break;
        case SearchMode.FileNameAndContent:
          break;
        default:
          break;
      }
    }

    private void RemoveContentsThatDoNotMatchSearchTerm()
    {
      if (!_search.FileResults.Any())
      {
        return;
      }

      var contentsToRemove = Children.Where(c => !ContainsOrIsAnyFileSearchResult(c))
                                     .ToArray();

      foreach (var content in contentsToRemove)
      {
        _uiDispatch.Do(() => Children.Remove(content));
      }
    }

    private bool ContainsOrIsAnyFileSearchResult(object child)
    {
      if (child is FileItemViewModel file)
      {
        return _search.FileResults.Any(m => m.File.Path.StartsWith(file.Path));
      }

      if (child is DirectoryTreeNodeViewModel directory)
      {
        return _search.FileResults.Any(m => m.File.Path.StartsWith(directory.Path));
      }

      return false;

    }

    private void HighlightSearchTerm()
    {
      return;
      var fileResults = _search.FileResults.ToArray();
      var searchTerms = fileResults.Select(r => r.Term).Distinct().OrderBy(t => t.Length).ToArray();
      Name = ViewModelUtils.GetHighlightMarkup(Name, searchTerms);
    }

    private void ResetName()
    {
      Name = Model.Name;
    }

    public override string ToString()
    {
      return $"{Name} ({Path})";
    }
  }
}
