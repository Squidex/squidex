// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Shared.Users;

namespace Squidex.Areas.IdentityServer.Controllers.Profile
{
    public sealed class ProfileVM
    {
        public string Id { get; set; }

        public string Email { get; set; }

        public string DisplayName { get; set; }

        public string ErrorMessage { get; set; }

        public string SuccessMessage { get; set; }

        public bool IsHidden { get; set; }

        public bool HasPassword { get; set; }

        public bool HasPasswordAuth { get; set; }

        public IReadOnlyList<ExternalLogin> ExternalLogins { get; set; }

        public IReadOnlyList<ExternalProvider> ExternalProviders { get; set; }
    }
}
