using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TicketApi.Domain.Repositories;
using TicketApi.Infrastructure.Persistence;
using TicketApi.Infrastructure.Repositories;

namespace TicketApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<TicketDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ITicketRepository, PostgresTicketRepository>();

        return services;
    }
}
