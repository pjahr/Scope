﻿using System.ComponentModel.Composition;
using System.Linq;
using Scope.Interfaces;

namespace Scope.FileViewer.DDS
{
  [Export]
  public class FileViewerFactory : IFileViewerFactory
  {
    private static readonly string[] _extensions;

    static FileViewerFactory()
    {
      _extensions = new[] {".dds"};
    }

    public bool CanHandle(IFile file)
    {
      return _extensions.Any(e => file.Name.EndsWith(e));
    }

    public IFileViewer Create(IFile file)
    {
      return new DdsFileViewer(file);
    }
  }
}
