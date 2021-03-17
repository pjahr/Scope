using System;
using System.Collections.Generic;
using System.Linq;
using Scope.Interfaces;
using Scope.Models.Interfaces;
using Scope.ViewModels.Factories;

namespace Scope.ViewModels
{
  internal class DirectoryTreeNodeViewModel : TreeNodeViewModel
  {
    private readonly ISearch _search;
    private readonly ISearchOptions _searchOptions;
    private readonly IUiDispatch _uiDispatch;
    private readonly IFileSystemTreeNodeViewModelFactory _fileSystemTreeNodeViewModelFactory;
    private bool _isLoadingChildren;

    public DirectoryTreeNodeViewModel(IDirectory directory,
                                      ISearch search,
                                      ISearchOptions searchOptions,
                                      IUiDispatch uiDispatch,
                                      IFileSystemTreeNodeViewModelFactory fileSystemTreeNodeViewModelFactory)
      : base(directory.Name,
             directory.Path,
             true)
    {
      Model = directory;

      _search = search;
      _searchOptions = searchOptions;
      _uiDispatch = uiDispatch;
      _fileSystemTreeNodeViewModelFactory = fileSystemTreeNodeViewModelFactory;

      _search.Finished += FilterContent;
      _search.Finished += HighlightSearchTerm;
      _search.ResultsCleared += ResetName;
      _search.Began += ResetChildren;

      HighlightSearchTerm();
    }

    private void ResetName()
    {
      Name = Model.Name;
    }

    private void HighlightSearchTerm()
    {
      var searchTerms = _search.CurrentSearchTerms
                               .Distinct()
                               .OrderBy(t => t.Length)
                               .ToArray();
      if (!searchTerms.Any())
      {
        searchTerms = new[] { "" };
      }

      Name = ViewModelUtils.GetHighlightMarkup(Name, searchTerms);
    }

    public IDirectory Model { get; }

    public bool IsLoadingChildren
    {
      get => _isLoadingChildren;
      set
      {
        if (value == _isLoadingChildren)
        {
          return;
        }

        _isLoadingChildren = value;

        RaisePropertyChanged(nameof(IsLoadingChildren));
      }
    }

    protected override void OnDisposing()
    {
      base.OnDisposing();

      _search.Finished -= FilterContent;
      _search.Finished -= HighlightSearchTerm;
      _search.ResultsCleared -= ResetName;
      _search.Began -= ResetChildren;
    }

    protected override ITreeNodeViewModel[] LoadChildren()
    {
      IsLoadingChildren = true;
      var contents = new List<ITreeNodeViewModel>();
      var pathes = _search.FileResults.Select(r => r.File.Path).ToArray();

      foreach (var directory in GetDirectories())
      {
        contents.Add(_fileSystemTreeNodeViewModelFactory.Create(directory));
      }

      foreach (var file in GetFiles())
      {
        contents.Add(_fileSystemTreeNodeViewModelFactory.Create(file));
      }
      IsLoadingChildren = false;
      return contents.ToArray();
    }

    private IEnumerable<IFile> GetFiles()
    {
      var allFiles = Model.Files.OrderBy(f => f.Name);

      return _search.FileResults.Any()
              ? allFiles.Where(f => _search.ResultIds.Contains(f.Index))
              : allFiles;
    }

    private IEnumerable<IDirectory> GetDirectories()
    {
      var allDirectories = Model.Directories.OrderBy(d => d.Name);

      return _search.FileResults.Any()
               ? allDirectories.Where(d => _search.ResultPaths.Any(path => path.StartsWith(d.Path)))
               : allDirectories;
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

      throw new NotImplementedException();
      foreach (var content in contentsToRemove)
      {
        //_uiDispatch.Do(() => Children.Remove(content));
      }
    }

    private bool ContainsOrIsAnyFileSearchResult(ITreeNodeViewModel child)
    {
      return _search.FileResults
                    .Any(m => m.File.Path.StartsWith(child.Path));
    }

    public override string ToString()
    {
      return $"{Name} ({Path})";
    }
  }
}
