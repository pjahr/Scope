﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Scope.FileViewer.ChCr;
using Scope.Interfaces;
using Scope.Utils;

namespace Scope.FileViewer.Text
{
  [Export]
  public class ChCrFileViewerFactory : IFileViewerFactory
  {
    private readonly Dictionary<string, Func<IFile, IFileViewer>> _factories;

    public ChCrFileViewerFactory()
    {
      _factories = new Dictionary<string, Func<IFile, IFileViewer>>
                   {
                     {"pla", f => new ChCrFileViewModel(f)},
                   };
    }

    public FileCategory Category => FileCategory.Text;

    public bool CanHandle(IFile file)
    {
      return _factories.ContainsKey(file.Name.GetExtension());
    }

    public IFileViewer Create(IFile file)
    {
      return _factories[file.Name.GetExtension().ToLower()](file);
    }
  }
}