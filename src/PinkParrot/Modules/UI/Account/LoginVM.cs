// ==========================================================================
//  LoginVM.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;

namespace PinkParrot.Modules.UI.Account
{
    public class LoginVM
    {
        public string ReturnUrl { get; set; }

        public IEnumerable<ExternalProvider> ExternalProviders { get; set; }
    }
}
