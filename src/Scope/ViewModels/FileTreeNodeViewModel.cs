using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Scope.Interfaces;

namespace Scope.ViewModels
{
  internal class FileTreeNodeViewModel : TreeNodeViewModel
  {
    public FileTreeNodeViewModel(IFile file) : base(file.Name, file.Path)
    {
      Model = file;

      var compressed = file.BytesCompressed.ToFileSize()
                           .Split(' ');
      CompressedSizeValue = compressed[0];
      CompressedSizeUnit = compressed[1];

      var uncompressed = file.BytesUncompressed.ToFileSize()
                             .Split(' ');

      UncompressedSizeValue = uncompressed[0];
      UncompressedSizeUnit = uncompressed[1];
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
  }
}
