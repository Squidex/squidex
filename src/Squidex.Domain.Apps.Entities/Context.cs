// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Security.Claims;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure.Security;
using Squidex.Shared.Identity;

namespace Squidex.Domain.Apps.Entities
{
    public sealed class Context
    {
        public IAppEntity App { get; set; }

        public ClaimsPrincipal User { get; set; }

        public PermissionSet Permissions { get; set; }

        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        public Context()
        {
        }

        public Context(ClaimsPrincipal user, IAppEntity app)
        {
            User = user;

            if (user != null)
            {
                Permissions = user.Permissions();
            }

            App = app;
        }

        public bool IsFrontendClient
        {
            get { return User != null && User.IsInClient("squidex-frontend"); }
        }
    }
}
