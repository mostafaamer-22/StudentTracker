using Scrutor;
using Application.Abstractions.Messaging;
using StudentTracker.Application.Abstractions.Messaging;
using StudentTracker.Application.Features.Students.DTOs;

namespace StudentTracker.Api.Configurations;

internal static class AppServicesDIConfig
{
    internal static IServiceCollection AddAppServicesDIConfig(this IServiceCollection services)
    {
        services
            .Scan(
                selector => selector
                    .FromAssemblies(
                        StudentTracker.Infrastructure.AssemblyReference.Assembly,
                        Application.AssemblyReference.Assembly)
                    .AddClasses(classes => classes
                        .Where(type =>
                            // Include handlers
                            (type.Name.EndsWith("Handler") &&
                            (type.GetInterfaces().Any(i =>
                                i.IsGenericType &&
                                (i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>) ||
                                 i.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
                                 i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
                                 i.GetGenericTypeDefinition() == typeof(IPaginateQueryHandler<,>) ||
                                 i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>))))) ||
                            // Include other services (repositories, seeders, etc.)
                            (!type.Name.EndsWith("Dto") &&
                             !type.Name.EndsWith("Query") &&
                             !type.Name.EndsWith("Command") &&
                             type.GetInterfaces().Length > 0)))
                    .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());

        return services;
    }
}
