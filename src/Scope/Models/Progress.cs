using System;
using System.ComponentModel.Composition;
using Scope.Models.Interfaces;
using Scope.Utils;

namespace Scope.Models
{
  [Export]
  internal class Progress : IProgress
  {
    public double Value { get; private set; }
    public bool InProgress { get; private set; }
    public bool Indetermined { get; private set; }

    public event Action Changed;

    public void Set(double value)
    {
      if (value < 0 || value > 1)
      {
        throw new ArgumentException("Value is expected to be between 0 and 1.");
      }

      Value = value;
      InProgress = true;
      Indetermined = false;

      Changed.Raise();
    }

    public void SetIndetermined()
    {
      InProgress = true;
      Indetermined = true;

      Changed.Raise();
    }

    public void Stop()
    {
      InProgress = false;
      Indetermined = false;
      Value = 0;

      Changed.Raise();
    }
  }
}
