using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scope.Interfaces;
using Scope.Models.Interfaces;

namespace Scope.ViewModels
{
  internal class FileTreeNodeViewModel : TreeNodeViewModel
  {
    private readonly ISearch _search;

    public FileTreeNodeViewModel(IFile file,
                                 ISearch search) : base(file.Name, file.Path)
    {
      Model = file;
      _search = search;
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

      HighlightSearchTerm();
    }

    public IFile Model { get; }
    public string CompressedSizeValue { get; }
    public string CompressedSizeUnit { get; }
    public string UncompressedSizeValue { get; }
    public string UncompressedSizeUnit { get; }

    public override bool HasDummyChild => false;

    protected override void LoadChildren() { }

    protected override void ResetChildren() { }

    public override Task<List<TreeNodeViewModel>> LoadChildrenListAsync()
    {
      throw new NotImplementedException();
    }

    private void HighlightSearchTerm()
    {
      Name = ViewModelUtils.GetHighlightMarkup(Name, _search.Results.Select(r => r.Term).Distinct().OrderBy(t => t.Length).ToArray());
    }

    private void ResetName()
    {
      Name = Model.Name;
    }
  }
}
