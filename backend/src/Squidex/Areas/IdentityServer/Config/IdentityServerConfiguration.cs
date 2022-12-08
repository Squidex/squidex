// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using OpenIddict.Abstractions;
using Squidex.Domain.Users.InMemory;
using Squidex.Web;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Squidex.Areas.IdentityServer.Config;

public static class IdentityServerConfiguration
{
    public sealed class Scopes : InMemoryScopeStore
    {
        public Scopes()
            : base(BuildScopes())
        {
        }

        private static IEnumerable<(string, OpenIddictScopeDescriptor)> BuildScopes()
        {
            yield return (Constants.ScopeApi, new OpenIddictScopeDescriptor
            {
                Name = Constants.ScopeApi,
                Resources =
                {
                    Permissions.Prefixes.Scope + Constants.ScopeApi
                }
            });
        }
    }
}
