using DeploymentApp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Squidex.ICIS.Deployment
{
    public static class DeploymentServiceExtensions
    {
        public static void AddDeployment(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<DeploymentOptions>(
                config.GetSection("deployment"));

            services.AddSingleton<DeploymentService>();
        }
    }
}
