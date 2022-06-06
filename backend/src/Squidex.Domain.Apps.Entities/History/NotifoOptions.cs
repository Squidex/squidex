// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.History
{
    public sealed class NotifoOptions
    {
        public string AppId { get; set; }

        public string ApiKey { get; set; }

        public string ApiUrl { get; set; } = "https://app.notifo.io";

        public bool Debug { get; set; }

        public bool IsConfigured()
        {
            return
                !string.IsNullOrWhiteSpace(ApiKey) &&
                !string.IsNullOrWhiteSpace(ApiUrl) &&
                !string.IsNullOrWhiteSpace(AppId);
        }
    }
}
