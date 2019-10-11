using Microsoft.Extensions.DependencyInjection;
using Squidex.Web;

namespace Squidex.ICIS.UI
{
    public static class UIServiceCollectionExtensions
    {
        public static void AddUI(this IServiceCollection services)
        {
            services.AddSingleton<ICustomLinkExtension, ICISLinkExtension>();
        }
    }
}
