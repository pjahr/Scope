using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scope.Interfaces;
using Scope.Models.Interfaces;
using Scope.Utils;

namespace Scope.ViewModels
{
  internal class FileTreeNodeViewModel : TreeNodeViewModel
  {
    private readonly ISearch _search;
    private readonly ISearchOptions _searchOptions;
    private readonly IUiDispatch _uiDispatch;
    private IFileSubStructureProvider[] _fileSubStructureProviders;

    public FileTreeNodeViewModel(IFile file,
                                 ISearch search,
                                 ISearchOptions searchOptions,
                                 IUiDispatch uiDispatch,
                                 IFileSubStructureProvider[] fileSubStructureProviders) : base(file.Name, file.Path)
    {
      Model = file;
      _search = search;
      _searchOptions = searchOptions;
      _uiDispatch = uiDispatch;
      var compressed = file.BytesCompressed.ToFileSize()
                           .Split(' ');
      CompressedSizeValue = compressed[0];
      CompressedSizeUnit = compressed[1];

      var uncompressed = file.BytesUncompressed.ToFileSize()
                             .Split(' ');

      UncompressedSizeValue = uncompressed[0];
      UncompressedSizeUnit = uncompressed[1];

      _fileSubStructureProviders = fileSubStructureProviders.Where(f => f.ApplicableFileExtension == System.IO.Path.GetExtension(file.Name)).ToArray();

      _search.Finished += HighlightSearchTerm;
      _search.ResultsCleared += ResetName;

      HighlightSearchTerm();
    }

    public IFile Model { get; }
    public string CompressedSizeValue { get; }
    public string CompressedSizeUnit { get; }
    public string UncompressedSizeValue { get; }
    public string UncompressedSizeUnit { get; }

    public override bool HasDummyChild => base.HasDummyChild;

    protected override void OnDisposing()
    {
      Console.WriteLine($"Disposing node: {Model}");
    }

    public override Task<List<TreeNodeViewModel>> LoadChildrenListAsync()
    {
      foreach (var item in Children)
      {
        item.Dispose();
      }
      Children.Clear();

      foreach (var nodeVm in GetContents())
      {
        Children.Add(nodeVm);
      }

      return Task.FromResult(Children.ToList());
    }

    private List<TreeNodeViewModel> GetContents()
    {
      var contents = new List<TreeNodeViewModel>();
      var pathes = _search.FileResults.Select(r => r.File.Path).ToArray();

      foreach (var directory in GetDirectories())
      {
        contents.Add(new DirectoryTreeNodeViewModel(directory, _search, _searchOptions, _uiDispatch, _fileSubStructureProviders));
      }

      foreach (var file in GetFiles())
      {
        contents.Add(new FileTreeNodeViewModel(file, _search, _searchOptions, _uiDispatch, _fileSubStructureProviders.ToArray()));
      }

      return contents;
    }

    private IEnumerable<IFile> GetFiles()
    {
      var files = _fileSubStructureProviders.Select(p => p.GetFiles(Model))
                                            .SelectMany(f => f);

      return _search.FileResults.Any()
             ? files.Where(f => _search.ResultIds.Contains(f.Index))
             : files;
    }

    private IEnumerable<IDirectory> GetDirectories()
    {
      var directories = _fileSubStructureProviders.Select(p => p.GetDirectories(Model))
                                                  .SelectMany(f => f);

      return _search.FileResults.Any()
             ? directories.Where(d => _search.ResultPaths.Any(path => path.StartsWith(d.Path)))
             : directories;
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

    private bool ContainsOrIsAnyFileSearchResult(TreeNodeViewModel child)
    {
      return _search.FileResults.Any(m => m.File.Path.StartsWith(child.Path));
    }

    private void HighlightSearchTerm()
    {
      Name = ViewModelUtils.GetHighlightMarkup(Name, _search.FileResults.Select(r => r.Term).Distinct().OrderBy(t => t.Length).ToArray());
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
