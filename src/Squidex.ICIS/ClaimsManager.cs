// ==========================================================================
//  ICIS Copyright
// ==========================================================================
//  Copyright (c) ICIS
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Users;
using Squidex.ICIS.Extensions;
using Squidex.ICIS.Interfaces;
using Squidex.ICIS.Models;
using Squidex.Infrastructure.Log;

namespace Squidex.ICIS
{
    public sealed class ClaimsManager : IClaimsManager
    {
        public const string ClientId = "vega.cms";

        private readonly UserManager<IdentityUser> userManager;
        private readonly AppProvider appProvider;
        private readonly IUserFactory userFactory;
        private readonly ISemanticLog log;
        private readonly IConfiguration config;

        public ClaimsManager(IServiceProvider services)
        {
            userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            appProvider = services.GetRequiredService<AppProvider>();
            userFactory = services.GetRequiredService<IUserFactory>();
            config = services.GetRequiredService<IConfiguration>();

            log = services.GetRequiredService<ISemanticLog>();
        }

        public UserInfo CreateUserWithClaims(ClaimsIdentity identity)
        {
            var userInfo = new UserInfo(identity, userManager, appProvider, config);
            CreateUser(userInfo);
            CreateClaims(identity, userInfo);

            return userInfo;
        }

        private void CreateClaims(ClaimsIdentity identity, UserInfo userInfo)
        {
            var squidexClaims = userInfo.ToUserValues().ToClaims();
            foreach (var squidexClaim in from squidexClaim in squidexClaims
                let found = identity.HasClaim((claim) => AreClaimsEqual(claim, squidexClaim))
                where !found
                select squidexClaim)
            {
                identity.AddClaim(squidexClaim);
            }
        }

        private bool AreClaimsEqual(Claim identityClaim, Claim squidexClaim)
        {
            return identityClaim.Type.Equals(squidexClaim.Type, StringComparison.OrdinalIgnoreCase) &&
                   identityClaim.Value.Equals(squidexClaim.Value, StringComparison.OrdinalIgnoreCase);
        }

        private void CreateUser(UserInfo userInfo)
        {
            Task.Run(async () =>
            {
                var userValues = userInfo.ToUserValues();
                if (userManager.SupportsQueryableUsers && !DoesUserExists(userValues.Email))
                {
                    try
                    {
                        await userManager.CreateAsync(userFactory, userValues);
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, w => w
                            .WriteProperty("action", "createICISUser")
                            .WriteProperty("status", "failed"));
                    }
                }
            }).Wait();
        }

        private bool DoesUserExists(string email)
        {
            var result = false;
            Task.Run(async () =>
            {
                try
                {
                    var value = await userManager.FindByEmailAsync(email);
                    result = value != null;
                }
                catch (Exception ex)
                {
                    log.LogError(ex, w => w
                        .WriteProperty("action", "checkingICISUser")
                        .WriteProperty("status", "failed"));
                }
            }).Wait();
            return result;
        }
    }
}