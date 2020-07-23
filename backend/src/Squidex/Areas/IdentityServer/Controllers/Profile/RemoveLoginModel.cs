// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Areas.IdentityServer.Controllers.Profile
{
    public class RemoveLoginModel
    {
        [Required]
        public string LoginProvider { get; set; }

        [Required]
        public string ProviderKey { get; set; }
    }
}
