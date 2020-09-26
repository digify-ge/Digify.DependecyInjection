using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Digify.DependecyInjection
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddAutoDependency(this IServiceCollection serviceCollection)
        {
            if(serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>() == null)
            {
                serviceCollection.AddLogging();
            }
            var assemblyProvider = new AssemblyProvider(serviceCollection.BuildServiceProvider());
            var assemblies = assemblyProvider.GetAssemblies(string.Empty);
            var dependencies = BuildBlueprint(assemblies, IsDependency);
            foreach (var dependency in dependencies)
            {
                foreach (var interfaceType in dependency.GetInterfaces())
                {
                    // GetInterfaces returns the full hierarchy of interfaces
                    if (interfaceType == typeof(ISingletonDependency) ||
                        interfaceType == typeof(ITransientDependency) ||
                        interfaceType == typeof(IDependency) ||
                        !typeof(IDependency).IsAssignableFrom(interfaceType))
                    {
                        continue;
                    }

                    if (typeof(ISingletonDependency).IsAssignableFrom(interfaceType))
                    {
                        serviceCollection.AddSingleton(interfaceType, dependency);
                    }
                    else if (typeof(ITransientDependency).IsAssignableFrom(interfaceType))
                    {
                        serviceCollection.AddTransient(interfaceType, dependency);
                    }
                    else if (typeof(IDependency).IsAssignableFrom(interfaceType))
                    {
                        serviceCollection.AddScoped(interfaceType, dependency);
                    }
                    break;
                }
            }
           
            return serviceCollection;
        }
        private static bool IsDependency(Type type)
        {
            return
                typeof(IDependency).IsAssignableFrom(type);
        }
        private static IEnumerable<Type> BuildBlueprint(IEnumerable<Assembly> features,Func<Type, bool> predicate )
        {
            // Load types excluding the replaced types
            return features.SelectMany(
                feature => feature.ExportedTypes
                    .Where(predicate)
                    .Where(t => t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract)
                .ToArray());
        }
    }
}
