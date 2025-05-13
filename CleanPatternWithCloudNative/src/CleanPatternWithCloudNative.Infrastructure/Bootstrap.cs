using CleanPatternWithCloudNative.Application.Abstractions.Caching;
using CleanPatternWithCloudNative.Application.Abstractions.Clock;
using CleanPatternWithCloudNative.Domain.Abstract;
using CleanPatternWithCloudNative.Domain.Repositories;
using CleanPatternWithCloudNative.Infrastructure.Caching;
using CleanPatternWithCloudNative.Infrastructure.Clock;
using CleanPatternWithCloudNative.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanPatternWithCloudNative.Infrastructure
{
    public static class Bootstrap
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddScoped<IDateTimeProvider, DateTimeProvider>();

            string dbConnectionString = configuration.GetConnectionString(Constants.DatabaseConnectionstringName)
                ?? throw new ArgumentNullException(nameof(configuration));

            services.AddDbContext<ApplicationDbContext>(options => options
                .UseNpgsql(dbConnectionString));

            string cacheConnectionString = configuration.GetConnectionString(Constants.CacheConnectionstringName)
                ?? throw new ArgumentNullException(nameof(configuration));

            services.AddStackExchangeRedisCache(options => options.Configuration = cacheConnectionString);

            services
                .AddScoped<IProductsRepository, ProductsRepository>()
                .AddScoped<ICacheService, CacheService>();

            return services;
        }
    }
}