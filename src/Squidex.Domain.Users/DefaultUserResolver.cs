// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Squidex.Infrastructure;
using Squidex.Shared.Users;

namespace Squidex.Domain.Users
{
    public sealed class DefaultUserResolver : IUserResolver
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IUserFactory userFactory;

        public DefaultUserResolver(UserManager<IdentityUser> userManager, IUserFactory userFactory)
        {
            Guard.NotNull(userManager, nameof(userManager));
            Guard.NotNull(userFactory, nameof(userFactory));

            this.userManager = userManager;
            this.userFactory = userFactory;
        }

        public async Task<bool> CreateUserIfNotExists(string email)
        {
            var user = userFactory.Create(email);

            try
            {
                var result = await userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    await userManager.UpdateAsync(user, new UserValues { DisplayName = email });
                }

                return result.Succeeded;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IUser> FindByIdOrEmailAsync(string idOrEmail)
        {
            if (userFactory.IsId(idOrEmail))
            {
                return await userManager.FindByIdWithClaimsAsync(idOrEmail);
            }
            else
            {
                return await userManager.FindByEmailWithClaimsAsyncAsync(idOrEmail);
            }
        }

        public async Task<List<IUser>> QueryByEmailAsync(string email)
        {
            var result = await userManager.QueryByEmailAsync(email);

            return result.OfType<IUser>().ToList();
        }
    }
}
