﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Squidex.Config;
using Squidex.Domain.Users;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.Areas.IdentityServer.Config;

public sealed class CreateAdminInitializer(IServiceProvider serviceProvider) : IInitializable
{
    public int Order => int.MaxValue;

    public async Task InitializeAsync(
        CancellationToken ct)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var identityOptions = serviceProvider.GetRequiredService<IOptions<MyIdentityOptions>>().Value;

        IdentityModelEventSource.ShowPII = identityOptions.ShowPII;

        if (!identityOptions.IsAdminConfigured())
        {
            return;
        }

        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var adminEmail = identityOptions.AdminEmail;
        var adminPass = identityOptions.AdminPassword;

        var isEmpty = await IsEmptyAsync(userService);

        if (!isEmpty && !identityOptions.AdminRecreate)
        {
            return;
        }

        try
        {
            var user = await userService.FindByEmailAsync(adminEmail, ct);

            if (user != null)
            {
                if (identityOptions.AdminRecreate)
                {
                    var permissions = CreatePermissions(user.Claims.Permissions(), identityOptions);

                    var values = new UserValues
                    {
                        Password = adminPass,
                        Permissions = permissions,
                    };

                    await userService.UpdateAsync(user.Id, values, ct: ct);
                }
            }
            else
            {
                var permissions = CreatePermissions(PermissionSet.Empty, identityOptions);

                var values = new UserValues
                {
                    Password = adminPass,
                    Permissions = permissions,
                    DisplayName = adminEmail,
                };

                await userService.CreateAsync(adminEmail, values, ct: ct);
            }
        }
        catch (Exception ex)
        {
            var log = serviceProvider.GetRequiredService<ILogger<CreateAdminInitializer>>();

            log.LogError(ex, "Failed to create administrator.");
        }
    }

    private static PermissionSet CreatePermissions(PermissionSet permissions, MyIdentityOptions identityOptions)
    {
        permissions = permissions.Add(PermissionIds.Admin);

        foreach (var app in identityOptions.AdminApps.OrEmpty())
        {
            permissions = permissions.Add(PermissionIds.ForApp(PermissionIds.AppAdmin, app));
        }

        return permissions;
    }

    private static async Task<bool> IsEmptyAsync(IUserService userService)
    {
        var users = await userService.QueryAsync(take: 1);

        return users.Total == 0;
    }
}
