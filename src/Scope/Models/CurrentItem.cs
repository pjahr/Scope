using System;
using System.ComponentModel.Composition;
using Scope.Interfaces;
using Scope.Models.Interfaces;
using Scope.Utils;

namespace Scope.Models
{
  [Export]
  internal class CurrentItem : ICurrentItem
  {
    public IFile CurrentFile { get; private set; }
    public IDirectory CurrentDirectory { get; private set; }

    public event Action Changed;

    public void ChangeTo(IFile file)
    {
      if (CurrentFile == file)
      {
        return;
      }

      CurrentFile = file;
      CurrentDirectory = null;

      Changed.Raise();
    }

    public void ChangeTo(IDirectory directory)
    {
      if (CurrentDirectory == directory)
      {
        return;
      }

      CurrentDirectory = directory;
      CurrentFile = null;

      Changed.Raise();
    }

    public void Clear()
    {
      CurrentDirectory = null;
      CurrentFile = null;

      Changed.Raise();
    }
  }
}
