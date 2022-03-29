using System;

namespace Scope.Models.Interfaces
{
  public interface IProgress
  {
    void Stop();

    void SetIndetermined();

    /// <summary>
    /// Set the progress of a long-running operation as a fraction of 1.
    /// </summary>
    void Set(double value);

    double Value { get; }
    bool InProgress { get; }
    bool Indetermined { get; }

    event Action Changed;
  }
}
