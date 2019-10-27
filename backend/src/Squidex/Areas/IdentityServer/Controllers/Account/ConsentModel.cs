// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.IdentityServer.Controllers.Account
{
    public sealed class ConsentModel
    {
        public bool ConsentToPersonalInformation { get; set; }

        public bool ConsentToAutomatedEmails { get; set; }

        public bool ConsentToCookies { get; set; }
    }
}
