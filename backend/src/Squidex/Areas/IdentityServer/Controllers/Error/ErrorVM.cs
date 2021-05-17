// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.IdentityServer.Controllers.Error
{
    public class ErrorVM
    {
        public string? ErrorMessage { get; set; }

        public string? ErrorCode { get; set; } = "400";
    }
}
