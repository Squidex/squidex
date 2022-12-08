// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Users;
using Squidex.Shared.Users;

namespace Squidex.Config.Authentication;

public static class IdentityServices
{
    public static void AddSquidexIdentity(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<MyIdentityOptions>(config,
            "identity");

        services.AddSingletonAs<DefaultUserResolver>()
            .AsOptional<IUserResolver>();

        services.AddSingletonAs<DefaultUserPictureStore>()
            .AsOptional<IUserPictureStore>();
    }
}
