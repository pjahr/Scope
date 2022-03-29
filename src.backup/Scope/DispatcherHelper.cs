﻿using System;
using System.Text;
using System.Windows.Threading;

namespace Scope
{
  /// <summary>
  /// Helper class for dispatcher operations on the UI thread.
  /// Source:
  /// https://raw.githubusercontent.com/lbugnion/mvvmlight/master/GalaSoft.MvvmLight/GalaSoft.MvvmLight.Platform%20(NET45)/Threading/DispatcherHelper.cs
  /// </summary>
  public static class DispatcherHelper
  {
    /// <summary>
    /// Gets a reference to the UI thread's dispatcher, after the
    /// <see cref="Initialize" /> method has been called on the UI thread.
    /// </summary>
    // ReSharper disable InconsistentNaming
    public static Dispatcher UIDispatcher
    // ReSharper restore InconsistentNaming
    {
      get;
      private set;
    }

    /// <summary>
    /// Executes an action on the UI thread. If this method is called
    /// from the UI thread, the action is executed immendiately. If the
    /// method is called from another thread, the action will be enqueued
    /// on the UI thread's dispatcher and executed asynchronously.
    /// <para>For additional operations on the UI thread, you can get a
    /// reference to the UI thread's dispatcher thanks to the property
    /// <see cref="UIDispatcher" /></para>.
    /// </summary>
    /// <param name="action">The action that will be executed on the UI
    /// thread.</param>
    // ReSharper disable InconsistentNaming
    public static void CheckBeginInvokeOnUI(Action action)
    // ReSharper restore InconsistentNaming
    {
      if (action == null)
      {
        return;
      }

      CheckDispatcher();

      if (UIDispatcher.CheckAccess())
      {
        action();
      }
      else
      {
        UIDispatcher.BeginInvoke(action);
      }
    }

    private static void CheckDispatcher()
    {
      if (UIDispatcher == null)
      {
        var error = new StringBuilder("The DispatcherHelper is not initialized.");
        error.AppendLine();
        error.Append("Call DispatcherHelper.Initialize() in the static App constructor.");

        throw new InvalidOperationException(error.ToString());
      }
    }

    /// <summary>
    /// Invokes an action asynchronously on the UI thread.
    /// </summary>
    /// <param name="action">The action that must be executed.</param>
    /// <returns>An object, which is returned immediately after BeginInvoke is called, that can be used to interact
    ///  with the delegate as it is pending execution in the event queue.</returns>
    public static DispatcherOperation RunAsync(Action action)
    {
      CheckDispatcher();

      return UIDispatcher.BeginInvoke(action);
    }

    /// <summary>
    /// This method should be called once on the UI thread to ensure that
    /// the <see cref="UIDispatcher" /> property is initialized.
    /// <para>In a Silverlight application, call this method in the
    /// Application_Startup event handler, after the MainPage is constructed.</para>
    /// <para>In WPF, call this method on the static App() constructor.</para>
    /// </summary>
    public static void Initialize()
    {
      if (UIDispatcher != null
          && UIDispatcher.Thread.IsAlive)
      {
        return;
      }

      UIDispatcher = Dispatcher.CurrentDispatcher;
    }

    /// <summary>
    /// Resets the class by deleting the <see cref="UIDispatcher"/>
    /// </summary>
    public static void Reset()
    {
      UIDispatcher = null;
    }
  }
}
