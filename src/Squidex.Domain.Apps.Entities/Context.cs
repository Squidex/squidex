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
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.Domain.Apps.Entities
{
    public sealed class Context
    {
        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        public IAppEntity App { get; set; }

        public ClaimsPrincipal User { get; set; }

        public PermissionSet Permissions
        {
            get { return User?.Permissions() ?? PermissionSet.Empty; }
        }

        public Context()
        {
        }

        public Context(ClaimsPrincipal user, IAppEntity app)
        {
            User = user;

            App = app;
        }

        public bool IsFrontendClient
        {
            get { return User != null && User.IsInClient(DefaultClients.Frontend); }
        }
    }
}
