using System;

namespace Scope.Interfaces
{
  /// <summary>
  /// Allows sheduling calls on the UI thread (the main app event loop).
  /// This is necessary when view models/views react to model events that occur on other (worker) threads.
  /// 
  /// </summary>
  /// <example>
  /// 
  /// View models which require marshalling on model events can retrieve this service via constructor injection and
  /// hook the event like this:
  /// 
  /// <code>
  /// 
  /// IUiDispatch _uiDispatch; // initialized during ctor
  /// 
  /// _someService.EventOnAnotherThread += _uiDispatch.Do( () => UpdateSomeUiElement );  // this will safely cross-thread-access your control
  /// 
  /// </code>
  /// 
  /// </example>
  public interface IUiDispatch
  {
    /// <summary>
    /// Takes care that the provided code is called on the UI thread to avoid cross-thread access to UI controls. 
    /// </summary>
    void Do(Action action);
  }
}
