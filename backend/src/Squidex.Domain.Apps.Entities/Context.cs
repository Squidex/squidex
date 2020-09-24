// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Security.Claims;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;
using ClaimsPermissions = Squidex.Infrastructure.Security.PermissionSet;
using P = Squidex.Shared.Permissions;

namespace Squidex.Domain.Apps.Entities
{
    public sealed class Context
    {
        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public IAppEntity App { get; set; }

        public ClaimsPrincipal User { get; }

        public ClaimsPermissions Permissions { get; }

        public bool IsFrontendClient { get; }

        public Context(ClaimsPrincipal user)
        {
            Guard.NotNull(user, nameof(user));

            User = user;

            Permissions = User.Permissions();

            IsFrontendClient = User.IsInClient(DefaultClients.Frontend);
        }

        public Context(ClaimsPrincipal user, IAppEntity app)
            : this(user)
        {
            App = app;
        }

        public static Context Anonymous()
        {
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            return new Context(claimsPrincipal);
        }

        public static Context Admin()
        {
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            claimsIdentity.AddClaim(new Claim(SquidexClaimTypes.Permissions, P.All));

            return new Context(claimsPrincipal);
        }

        public Context Clone()
        {
            var clone = new Context(User, App);

            foreach (var (key, value) in Headers)
            {
                clone.Headers[key] = value;
            }

            return clone;
        }
    }
}
