using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Scope.Interfaces;
using Scope.Models.Interfaces;
using Scope.Utils;

namespace Scope.Models
{
  [Export]
  internal class SelectedItems : ISelectedItems
  {
    private readonly List<IDirectory> _directories = new List<IDirectory>();
    private readonly List<IFile> _files = new List<IFile>();

    public IReadOnlyList<IDirectory> Directories => _directories;
    public IReadOnlyList<IFile> Files => _files;

    public event Action Changed;

    public void Add(params IDirectory[] directories)
    {
      _directories.AddRange(directories);
      Changed.Raise();
    }

    public void Add(params IFile[] files)
    {
      _files.AddRange(files);
      Changed.Raise();
    }

    public void Clear()
    {
      _directories.Clear();
      _files.Clear();

      Changed.Raise();
    }

    public void Remove(params IDirectory[] directories)
    {
      foreach (var item in directories)
      {
        _directories.Remove(item);
      }

      Changed.Raise();
    }

    public void Remove(params IFile[] files)
    {
      foreach (var item in files)
      {
        _files.Remove(item);
      }

      Changed.Raise();
    }
  }
}
