// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body

namespace Squidex.Domain.Users;

public sealed class DefaultUserResolver : IUserResolver
{
    private readonly IServiceProvider serviceProvider;

    public DefaultUserResolver(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public async Task<(IUser? User, bool Created)> CreateUserIfNotExistsAsync(string email, bool invited = false,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(email);

        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            try
            {
                var values = new UserValues
                {
                    Invited = invited
                };

                var user = await userService.CreateAsync(email, values, ct: ct);

                return (user, true);
            }
            catch
            {
            }

            var found = await FindByIdOrEmailAsync(email, ct);

            return (found, false);
        }
    }

    public async Task SetClaimAsync(string id, string type, string value, bool silent = false,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(id);
        Guard.NotNullOrEmpty(type);
        Guard.NotNullOrEmpty(value);

        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            var values = new UserValues
            {
                CustomClaims = new List<Claim>
                {
                    new Claim(type, value)
                }
            };

            await userService.UpdateAsync(id, values, silent, ct);
        }
    }

    public async Task<IUser?> FindByIdAsync(string id,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(id);

        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            return await userService.FindByIdAsync(id, ct);
        }
    }

    public async Task<IUser?> FindByIdOrEmailAsync(string idOrEmail,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(idOrEmail);

        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            if (idOrEmail.Contains('@', StringComparison.Ordinal))
            {
                return await userService.FindByEmailAsync(idOrEmail, ct);
            }
            else
            {
                return await userService.FindByIdAsync(idOrEmail, ct);
            }
        }
    }

    public async Task<List<IUser>> QueryAllAsync(
        CancellationToken ct = default)
    {
        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            var result = await userService.QueryAsync(take: int.MaxValue, ct: ct);

            return result.ToList();
        }
    }

    public async Task<List<IUser>> QueryByEmailAsync(string email,
        CancellationToken ct = default)
    {
        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            var result = await userService.QueryAsync(email, ct: ct);

            return result.Where(x => !x.Claims.IsHidden()).ToList();
        }
    }

    public async Task<Dictionary<string, IUser>> QueryManyAsync(string[] ids,
        CancellationToken ct = default)
    {
        Guard.NotNull(ids);

        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            var result = await userService.QueryAsync(ids, ct);

            return result.OfType<IUser>().ToDictionary(x => x.Id);
        }
    }
}
