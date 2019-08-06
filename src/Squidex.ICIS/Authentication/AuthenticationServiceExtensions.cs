using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Squidex.ICIS.Authentication.User;

namespace Squidex.ICIS.Authentication
{
    public static class AuthenticationServiceExtensions
    {
        public static void AddAuthenticationServices(IServiceCollection services, IConfiguration config)
        {
            var identityOptions = config.GetSection("identity").Get<MyIdentityOptionsExtension>();
            services.AddGenesisAuthentication(identityOptions.IcisAuthServer);
        }

        private static void AddGenesisAuthentication(this IServiceCollection services, string authServer)
        {
            services.AddSingleton<IClaimsTransformation, ClaimsTransformer>();

            services.AddSingleton<IUserManager, UserManager>();

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

    }
}
