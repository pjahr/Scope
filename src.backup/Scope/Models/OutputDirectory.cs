using Scope.Models.Interfaces;
using Scope.Utils;
using System;
using System.ComponentModel.Composition;

namespace Scope.Models
{
  [Export]
  internal class OutputDirectory : IOutputDirectory
  {
    private string _path;

    public string Path
    {
      get => _path;
      set
      {
        if (_path == value)
        {
          return;
        }

        _path = value;
        Changed.Raise();
      }
    }

    public event Action Changed;
  }
}
