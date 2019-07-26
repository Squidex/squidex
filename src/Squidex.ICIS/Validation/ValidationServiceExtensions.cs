using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.Commands;

namespace Squidex.ICIS.Validation
{
    public static class ValidationServiceExtensions
    {
        public static void AddValidationServices(IServiceCollection services)
        {
            AddValidationCommandMiddleware(services);
        }

        private static void AddValidationCommandMiddleware(IServiceCollection services)
        {
            services.AddSingleton<ICustomCommandMiddleware, UniqueContentValidationCommand>();
        }
    }
}
