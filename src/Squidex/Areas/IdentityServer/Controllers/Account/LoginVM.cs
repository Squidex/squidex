// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Areas.IdentityServer.Controllers.Account
{
    public class LoginVM
    {
        public string ReturnUrl { get; set; }

        public bool IsLogin { get; set; }

        public bool IsFailed { get; set; }

        public bool HasPasswordAuth { get; set; }

        public bool HasPasswordAndExternal { get; set; }

        public IReadOnlyList<ExternalProvider> ExternalProviders { get; set; }
    }
}
