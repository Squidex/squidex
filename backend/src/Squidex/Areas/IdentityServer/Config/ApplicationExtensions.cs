// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using OpenIddict.Abstractions;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

namespace Squidex.Areas.IdentityServer.Config
{
    public static class ApplicationExtensions
    {
        public static OpenIddictApplicationDescriptor SetAdmin(this OpenIddictApplicationDescriptor application)
        {
            application.Properties[SquidexClaimTypes.Permissions] = CreateParameter(Enumerable.Repeat(Permissions.All, 1));

            return application;
        }

        public static OpenIddictApplicationDescriptor CopyClaims(this OpenIddictApplicationDescriptor application, IUser claims)
        {
            foreach (var group in claims.Claims.GroupBy(x => x.Type))
            {
                application.Properties[group.Key] = CreateParameter(group.Select(x => x.Value));
            }

            return application;
        }

        private static JsonElement CreateParameter(IEnumerable<string> values)
        {
            return (JsonElement)new OpenIddictParameter(values.ToArray());
        }

        public static IEnumerable<Claim> Claims(this IReadOnlyDictionary<string, JsonElement> properties)
        {
            foreach (var (key, value) in properties)
            {
                var values = (string[]?)new OpenIddictParameter(value);

                if (values != null)
                {
                    foreach (var claimValue in values)
                    {
                        if (key == SquidexClaimTypes.DisplayName)
                        {
                            yield return new Claim(OpenIdClaims.Name, claimValue);
                        }

                        yield return new Claim(key, claimValue);
                    }
                }
            }
        }
    }
}
