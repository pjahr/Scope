using System;
using System.ComponentModel.Composition;
using Scope.Interfaces;

namespace Scope.Models
{
  /// <summary>
  /// Uses a statically referenced UI dispatcher (and hides this fact so that global state is not referenced everywhere).
  /// See interface documentation for the purpose of this class.
  /// </summary>
  /// 
  /// <remarks>
  /// The static reference is initialized in the static constructor of <see cref="App"/>.
  /// </remarks>
  [Export]
  internal class UiDispatch : IUiDispatch
  {
    /// <inheritdoc/>
    public void Do(Action action)
    {
      DispatcherHelper.CheckBeginInvokeOnUI(action);
    }
  }
}
