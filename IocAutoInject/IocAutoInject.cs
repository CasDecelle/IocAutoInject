using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace IocAutoInject;

[AttributeUsage(AttributeTargets.Class)]
public class InjectAttribute<T> : InjectAttribute
{
    public InjectAttribute(ServiceLifetime lifetime = ServiceLifetime.Scoped) : base(lifetime) { }
}

[AttributeUsage(AttributeTargets.Class)]
public abstract class InjectAttribute : Attribute
{
    public ServiceLifetime Lifetime { get; init; }
    public InjectAttribute(ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        Lifetime = lifetime;
    }
}

public static class AutoInjectExtensions
{
    public static IServiceCollection AddInjectableServices(this IServiceCollection services)
    {
        var serviceTypes = GetAllAssemblies()
            .DiscoverServices();

        services.AddServices(serviceTypes);

        return services;
    }

    private static IEnumerable<Type> DiscoverServices(this IEnumerable<Assembly> assemblies)
    {        
        var services = assemblies
            .Where(a => !a.FullName!.Contains("System") && !a.FullName!.Contains("Microsoft"))
            .SelectMany(a => a.GetTypes())
            .Where(t => t.GetCustomAttribute(typeof(InjectAttribute), false) != null);

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services, IEnumerable<Type> types)
    {
        foreach (var type in types)
        {
            services.Add(GetServiceDescriptorFromType(type));
        }
        return services;
    }

    private static ServiceDescriptor GetServiceDescriptorFromType(Type service)
    {
        InjectAttribute attribute = (service.GetCustomAttribute(typeof(InjectAttribute), false) as InjectAttribute)!;
        Type interfaceType = attribute.GetType().GenericTypeArguments.Single();
        ServiceLifetime lifetime = attribute.Lifetime;
        return ServiceDescriptor.Describe(interfaceType, service, lifetime);
    }

    private static IEnumerable<Assembly> GetAllAssemblies()
    {
        var rootAssembly = Assembly.GetEntryAssembly();
        var discovered = new List<Assembly>();

        if (rootAssembly == null)
            return discovered;

        var visited = new HashSet<string>();
        var queue = new Queue<Assembly>();

        queue.Enqueue(rootAssembly);

        while (queue.Any())
        {
            var assembly = queue.Dequeue();
            var assemblyAdded = visited.Add(assembly.FullName!);

            if (assemblyAdded)
                discovered.Add(assembly);

            var references = assembly.GetReferencedAssemblies();
            foreach (var reference in references)
            {
                if (!visited.Contains(reference.FullName))
                    queue.Enqueue(Assembly.Load(reference));
            }
        }

        return discovered;
    }
}
