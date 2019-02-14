// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.Api.Controllers.News
{
    public sealed class MyNewsOptions
    {
        public string AppName { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public bool IsConfigured()
        {
            return
                !string.IsNullOrWhiteSpace(AppName) &&
                !string.IsNullOrWhiteSpace(ClientId) &&
                !string.IsNullOrWhiteSpace(ClientSecret);
        }
    }
}
