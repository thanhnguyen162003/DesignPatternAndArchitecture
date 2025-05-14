using Microsoft.EntityFrameworkCore;
using VerticalSliceApi.Constaints;

namespace VerticalSliceApi.Common.EfCore
{
    public static class DependencyInjectionExtensions
    {
        public static void AddAppDbContext(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            string dbConnectionString =
                configuration.GetConnectionString(Constaint.DatabaseConnectionstringName)
                ?? throw new ArgumentNullException(nameof(configuration));

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(dbConnectionString);
            });
        }
    }
}
