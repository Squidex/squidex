// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Squidex.Areas.IdentityServer.Controllers.Profile
{
    public sealed class ProfileVM
    {
        public string Id { get; set; }

        public string Email { get; set; }

        public string DisplayName { get; set; }

        public string? ClientSecret { get; set; }

        public string? ErrorMessage { get; set; }

        public string? SuccessMessage { get; set; }

        public bool IsHidden { get; set; }

        public bool HasPassword { get; set; }

        public bool HasPasswordAuth { get; set; }

        public List<UserProperty> Properties { get; set; }

        public IList<UserLoginInfo> ExternalLogins { get; set; }

        public IList<ExternalProvider> ExternalProviders { get; set; }
    }
}
