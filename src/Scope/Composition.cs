using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO.Abstractions;
using System.Linq;
using Scope.Interfaces;
using Scope.Views;
using Lamar;

namespace Scope
{
  internal class Composition : IDisposable
  {
    private readonly Container _container;

    public Composition()
    {
      var registry = new ServiceRegistry();

      registry.Scan(_ =>
      {
        _.TheCallingAssembly();
        _.AssembliesFromPath("Plugins");
        _.AddAllTypesOf<AppWindow>();
        _.RegisterConcreteTypesAgainstTheFirstInterface();
        _.WithDefaultConventions();
      });

      _container = new Container(registry);

      //// enable injection of multiple services with the same interface
      //container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));

      //// special register: 'real' file sytem
      //container.Register((ComponentRegistration<object>)Component.For(typeof(IFileSystem))
      //                                                           .ImplementedBy(typeof(FileSystem))
      //                                                           .LifeStyle.Singleton);

      //// register built-in app object graph
      //container.Register((BasedOnDescriptor)Classes.FromAssemblyContaining<Composition>()
      //                                             .IncludeNonPublicTypes()
      //                                             .Where(t => t.GetCustomAttributes(false)
      //                                                          .Any(a => a is ExportAttribute))
      //                                             .WithServiceSelf()
      //                                             .WithServiceAllInterfaces()
      //                                             .LifestyleSingleton());
      //// register plugins
      //var registrations =
      //  Classes.FromAssemblyInDirectory(new AssemblyFilter(@"Plugins"))
      //                           .IncludeNonPublicTypes()
      //                           .Pick()
      //                           .If(t => t.GetCustomAttributes(false)
      //                                     .Any(a => a is ExportAttribute))
      //                           .WithServiceAllInterfaces()
      //                           .LifestyleSingleton();
      //container.Register(registrations);      

      PluginResources = _container.GetAllInstances<IResourceDictionary>();
      MainWindow = _container.GetInstance<AppWindow>();
    }

    public AppWindow MainWindow { get; }
    public IEnumerable<IResourceDictionary> PluginResources { get; }

    public void Dispose()
    {
      _container.Dispose();
    }
  }
}
