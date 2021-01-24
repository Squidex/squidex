// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.IdentityServer.Controllers.Setup
{
    public sealed class SetupVM
    {
        public string Email { get; set; }

        public string BaseUrlCurrent { get; set; }

        public string BaseUrlConfigured { get; set; }

        public string? ErrorMessage { get; set; }

        public bool IsValidHttps { get; set; }

        public bool IsAssetStoreFtp { get; set; }

        public bool IsAssetStoreFile { get; set; }

        public bool EverybodyCanCreateApps { get; set; }

        public bool HasExternalLogin { get; set; }

        public bool HasPasswordAuth { get; set; }
    }
}
