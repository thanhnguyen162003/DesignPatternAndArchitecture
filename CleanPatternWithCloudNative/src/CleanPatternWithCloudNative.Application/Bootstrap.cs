using CleanPatternWithCloudNative.Application.Behaviors;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

namespace CleanPatternWithCloudNative.Application
{
    public static class Bootstrap
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services
                .AddMediatR(config =>
                {
                    config.RegisterServicesFromAssembly(typeof(Bootstrap).Assembly);
                    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
                });

            services
                .AddValidatorsFromAssembly(typeof(Bootstrap).Assembly);

            return services;
        }
    }
}