// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.History
{
    public sealed class NotifoOptions
    {
        public string AppId { get; set; }

        public string ApiKeyOwner { get; set; }

        public string ApiKeyWeb { get; set; }

        public string ApiUrl { get; set; } = "https://app.notifo.io";

        public bool IsConfigured()
        {
            return
                !string.IsNullOrWhiteSpace(ApiKeyOwner) &&
                !string.IsNullOrWhiteSpace(ApiKeyWeb) &&
                !string.IsNullOrWhiteSpace(ApiUrl) &&
                !string.IsNullOrWhiteSpace(AppId);
        }
    }
}
