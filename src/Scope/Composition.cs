using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Scope.Interfaces;
using Scope.Views;

namespace Scope
{
  internal class Composition : IDisposable
  {
    private const string PluginDirectory = "Plugins";

    private readonly WindsorContainer _container;

    public Composition()
    {
      var container = new WindsorContainer();

      // enable injection of multiple services with the same interface
      container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));

      // special register: 'real' file sytem
      container.Register((ComponentRegistration<object>)Component.For(typeof(IFileSystem))
                                                                 .ImplementedBy(typeof(FileSystem))
                                                                 .LifeStyle.Singleton);

      // register built-in app object graph
      container.Register((BasedOnDescriptor)Classes.FromAssemblyContaining<Composition>()
                                                   .IncludeNonPublicTypes()
                                                   .Where(t => t.GetCustomAttributes(false)
                                                                .Any(a => a is ExportAttribute))
                                                   .WithServiceSelf()
                                                   .WithServiceAllInterfaces()
                                                   .LifestyleSingleton());
      // register plugins
      //Assembly.LoadFrom("Plugins\\Scope.FileViewer.Text.dll");
      //Assembly.LoadFrom("Plugins\\Scope.FileViewer.DataForge.dll");
      //Assembly.LoadFrom("Plugins\\Scope.File.SOCPAK.dll");
      //Assembly.LoadFrom("Plugins\\Scope.FileViewer.DataForge.dll");
      //Assembly.LoadFrom("Plugins\\Scope.FileViewer.DataForge.dll");
      //var registrations =

      //  Classes.FromAssemblyInDirectory(new AssemblyFilter(@"Plugins"))
      //                           .IncludeNonPublicTypes()
      //                           .Pick()
      //                           .If(t => t.GetCustomAttributes(false)
      //                                     .Any(a => a is ExportAttribute))
      //                           .WithServiceAllInterfaces()
      //                           .LifestyleSingleton();
      //container.Register(registrations);

      var plugins = Directory.GetFiles(Path.Combine(AssemblyDirectory, PluginDirectory), "Scope.*.dll");

      foreach (var plugin in plugins)
      {
        container.Register(Classes.FromAssembly(Assembly.LoadFrom(plugin))
                                  .IncludeNonPublicTypes()
                                  .Pick()
                                  .If(t => t.GetCustomAttributes(false)
                                          .Any(a => a is ExportAttribute))
                                .WithServiceAllInterfaces());
      }

      _container = container;

      PluginResources = container.ResolveAll<IResourceDictionary>();
      MainWindow = _container.Resolve<AppWindow>();
    }

    public AppWindow MainWindow { get; }
    public IEnumerable<IResourceDictionary> PluginResources { get; }

    public void Dispose()
    {
      _container.Dispose();
    }

    static public string AssemblyDirectory
    {
      get
      {
        var codeBase = Assembly.GetExecutingAssembly().Location;

        var uri = new UriBuilder(codeBase);

        var path = Uri.UnescapeDataString(uri.Path);

        return Path.GetDirectoryName(path);
      }
    }
  }
}
