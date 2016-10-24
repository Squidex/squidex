// ==========================================================================
//  LoginVM.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Modules.UI.Account
{
    public class LoginVM
    {
        public string ReturnUrl { get; set; }

        public IEnumerable<ExternalProvider> ExternalProviders { get; set; }
    }
}
