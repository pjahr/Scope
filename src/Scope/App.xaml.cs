using System.Windows;

namespace Scope
{
  internal partial class App : Application
  {
    private Composition _composition;

    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);
      _composition = new Composition();

      // merge exported resource dictionaries from plugins into the application
      foreach (var resourceDictionary in _composition.PluginResources)
      {
        var rd = LoadComponent(resourceDictionary.Uri) as ResourceDictionary;
        Resources.MergedDictionaries.Add(rd);
      }

      MainWindow = _composition.MainWindow;
      MainWindow.Show();

      if (e.Args.Length == 1)
      {
        //mainWindow.OpenStarcitizen(e.Args[0]);
      }
    }

    protected override void OnExit(ExitEventArgs e)
    {
      base.OnExit(e);

      _composition.Dispose(); // disposes container and with it all disposable singletons
    }
  }
}
