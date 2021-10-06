using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO.Abstractions;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Scope.Interfaces;
using Scope.Views;

namespace Scope
{
  internal class Composition : IDisposable
  {
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
      var registrations =
        Classes.FromAssemblyInDirectory(new AssemblyFilter(@"Plugins"))
                                 .IncludeNonPublicTypes()
                                 .Pick()
                                 .If(t => t.GetCustomAttributes(false)
                                           .Any(a => a is ExportAttribute))
                                 .WithServiceAllInterfaces()
                                 .LifestyleSingleton();
      container.Register(registrations);

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
  }
}
