// ==========================================================================
//  ICIS Copyright
// ==========================================================================
//  Copyright (c) ICIS
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Squidex.ICIS.Actions.Kafka;
using Squidex.ICIS.Handlers;
using Squidex.ICIS.Interfaces;

namespace Squidex.ICIS.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddGenesisAuthentication(this IServiceCollection services, string authServer)
        {
            services.AddSingleton<IClaimsManager, ClaimsManager>();
            
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Audience = $"{authServer}/resources";
                    options.Authority = authServer;
                    options.RequireHttpsMetadata = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = authServer
                    };
                    options.Events = new AuthEventsHandler();
                })
                .AddCookie();
        }

        public static void AddKafkaRuleExtention(this IServiceCollection services, IConfiguration config)
        {
            var kafkaOptions = config.GetSection("kafka").Get<ICISKafkaOptions>();
            if (kafkaOptions.IsConfigured())
            {
                services.Configure<ICISKafkaOptions>(config.GetSection("kafka"));
                services.AddRuleAction<ICISKafkaAction, ICISKafkaActionHandler>();
            }
        }
    }
}