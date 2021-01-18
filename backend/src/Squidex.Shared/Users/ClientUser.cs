// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Security.Claims;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Shared.Identity;

namespace Squidex.Shared.Users
{
    public sealed class ClientUser : IUser
    {
        private readonly RefToken token;
        private readonly List<Claim> claims;

        public string Id
        {
            get { return token.Identifier; }
        }

        public string Email
        {
            get { return token.ToString(); }
        }

        public bool IsLocked
        {
            get { return false; }
        }

        public IReadOnlyList<Claim> Claims
        {
            get { return claims; }
        }

        public object Identity => throw new System.NotImplementedException();

        public ClientUser(RefToken token)
        {
            Guard.NotNull(token, nameof(token));

            this.token = token;

            claims = new List<Claim>
            {
                new Claim(OpenIdClaims.ClientId, token.Identifier),
                new Claim(SquidexClaimTypes.DisplayName, token.ToString())
            };
        }
    }
}
