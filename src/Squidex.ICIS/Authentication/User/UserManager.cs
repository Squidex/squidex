// ==========================================================================
//  ICIS Copyright
// ==========================================================================
//  Copyright (c) ICIS
//  All rights reserved.
// ==========================================================================

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Users;
using Squidex.Infrastructure.Log;

namespace Squidex.ICIS.Authentication.User
{
    public sealed class UserManager: IUserManager
    {
        public const string ClientId = "vega.cms";

        private readonly UserManager<IdentityUser> userManager;
        private readonly IUserFactory userFactory;
        private readonly ISemanticLog log;
        private readonly IConfiguration config;

        public UserManager(IServiceProvider services)
        {
            userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            userFactory = services.GetRequiredService<IUserFactory>();
            config = services.GetRequiredService<IConfiguration>();

            log = services.GetRequiredService<ISemanticLog>();
        }

        public UserInfo GetUserInfo(ClaimsIdentity identity)
        {
            var userInfo = new UserInfo(identity, config);

            userInfo.UserId = CreateUser(userInfo).Result.Id;
            return userInfo;
        }

        private async Task<IdentityUser> CreateUser(UserInfo userInfo)
        {

            var identityUser = new IdentityUser();

            var userValues = userInfo.ToUserValues();
            if (userManager.SupportsQueryableUsers && !(await DoesUserExist(userValues.Email)))
            {
                try
                {
                    identityUser = (await userManager.CreateAsync(userFactory, userValues))?.Identity;
                }
                catch (Exception ex)
                {
                    log.LogError(ex, w => w
                        .WriteProperty("action", "createICISUser")
                        .WriteProperty("status", "failed"));
                }
            }
            else
            {
                identityUser = await userManager.FindByEmailAsync(userValues.Email);
            }

            return identityUser;

        }

        private async Task<bool> DoesUserExist(string email)
        {
            var result = false;

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
            
            return result;
        }
    }
}