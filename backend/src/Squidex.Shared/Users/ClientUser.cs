// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
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
            get => token.Identifier;
        }

        public string Email
        {
            get => token.ToString();
        }

        public bool IsLocked
        {
            get => false;
        }

        public IReadOnlyList<Claim> Claims
        {
            get => claims;
        }

        public object Identity => throw new NotSupportedException();

        public ClientUser(RefToken token)
        {
            Guard.NotNull(token);

            this.token = token;

            claims = new List<Claim>
            {
                new Claim(OpenIdClaims.ClientId, token.Identifier),
                new Claim(SquidexClaimTypes.DisplayName, token.Identifier)
            };
        }
    }
}
