// ==========================================================================
//  IdentityServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Domain.Users;
using Squidex.Domain.Users.DataProtection.Orleans;
using Squidex.Shared.Users;

namespace Squidex.Config.Identity
{
    public static class IdentityServices
    {
        public static void AddMyIdentity(this IServiceCollection services)
        {
            services.AddIdentity<IUser, IRole>()
                .AddDefaultTokenProviders();
        }

        public static void AddMyDataProtectection(this IServiceCollection services, IConfiguration config)
        {
            var dataProtection = services.AddDataProtection().SetApplicationName("Squidex");

            services.AddSingleton<OrleansXmlRepository>();

            services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(s =>
            {
                return new ConfigureOptions<KeyManagementOptions>(options =>
                {
                    options.XmlRepository = s.GetRequiredService<OrleansXmlRepository>();
                });
            });
        }
    }
}
