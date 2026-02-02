using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EventBus.Extensions;

public static class EventBusExtensions
{
    public static IServiceCollection AddEventBusSubscriptions(this IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>)))
            .Where(t => !t.IsAbstract && !t.IsInterface);

        foreach (var handlerType in handlerTypes)
        {
            var interfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>));

            foreach (var @interface in interfaces)
            {
                services.AddTransient(@interface, handlerType);
            }
        }

        return services;
    }
}
