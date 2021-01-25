﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure;
using Squidex.Shared.Users;

#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body

namespace Squidex.Domain.Users
{
    public sealed class DefaultUserResolver : IUserResolver
    {
        private readonly IServiceProvider serviceProvider;

        public DefaultUserResolver(IServiceProvider serviceProvider)
        {
            Guard.NotNull(serviceProvider, nameof(serviceProvider));

            this.serviceProvider = serviceProvider;
        }

        public async Task<(IUser? User, bool Created)> CreateUserIfNotExistsAsync(string email, bool invited)
        {
            Guard.NotNullOrEmpty(email, nameof(email));

            using (var scope = serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                try
                {
                    var user = await userService.CreateAsync(email, new UserValues
                    {
                        Invited = invited
                    });

                    return (user, true);
                }
                catch
                {
                }

                var found = await FindByIdOrEmailAsync(email);

                return (found, false);
            }
        }

        public async Task SetClaimAsync(string id, string type, string value)
        {
            Guard.NotNullOrEmpty(id, nameof(id));
            Guard.NotNullOrEmpty(type, nameof(type));
            Guard.NotNullOrEmpty(value, nameof(value));

            using (var scope = serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                var values = new UserValues
                {
                    CustomClaims = new List<Claim>
                    {
                        new Claim(type, value)
                    }
                };

                await userService.UpdateAsync(id, values);
            }
        }

        public async Task<IUser?> FindByIdAsync(string id)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            using (var scope = serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                return await userService.FindByIdAsync(id);
            }
        }

        public async Task<IUser?> FindByIdOrEmailAsync(string idOrEmail)
        {
            Guard.NotNullOrEmpty(idOrEmail, nameof(idOrEmail));

            using (var scope = serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                if (idOrEmail.Contains("@"))
                {
                    return await userService.FindByEmailAsync(idOrEmail);
                }
                else
                {
                    return await userService.FindByIdAsync(idOrEmail);
                }
            }
        }

        public async Task<List<IUser>> QueryAllAsync()
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                var result = await userService.QueryAsync(take: int.MaxValue);

                return result.ToList();
            }
        }

        public async Task<List<IUser>> QueryByEmailAsync(string email)
        {
            Guard.NotNullOrEmpty(email, nameof(email));

            using (var scope = serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                var result = await userService.QueryAsync(email);

                return result.ToList();
            }
        }

        public async Task<Dictionary<string, IUser>> QueryManyAsync(string[] ids)
        {
            Guard.NotNull(ids, nameof(ids));

            using (var scope = serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                var result = await userService.QueryAsync(ids);

                return result.OfType<IUser>().ToDictionary(x => x.Id);
            }
        }
    }
}
