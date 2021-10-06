using System.Windows;

namespace Scope
{
  internal partial class App : Application
  {
    static App()
    {
      // This provides the UI Dispatcher globally (for UI thread marshalling when modifying UI controls from background threads or async tasks).
      // 
      // Please don't use this static class anywhere else (global state introduces dangerously hard coupling).
      // If needed, inject an instance of IUiDispatch and use its .Do(...) method instead. 
      DispatcherHelper.Initialize();
    }

    private Composition _composition;

    /// <summary>
    /// This is called at the start of the application.
    /// It composes the application and  loads the data templates and views from plugin resource dictionaries.
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);
      _composition = new Composition();

      // merge exported resource dictionaries from plugins into the application (so they are discoverable by WPF templating engine)
      foreach (var resourceDictionary in _composition.PluginResources)
      {
        var rd = LoadComponent(resourceDictionary.Uri) as ResourceDictionary;
        Resources.MergedDictionaries.Add(rd);
      }

      MainWindow = _composition.MainWindow;
      MainWindow.Show();

      if (e.Args.Length == 1)
      {
        //TODO: allow passing of the P4K file path via command line arguments to directly load a p4k (and later file association). 
      }
    }

    /// <summary>
    /// Disposes the composition, including all services that implement IDisposable.
    /// </summary>
    protected override void OnExit(ExitEventArgs e)
    {
      base.OnExit(e);

      _composition.Dispose(); // disposes container and with it all disposable singletons
    }
  }
}
