// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Rules.Action.Twitter
{
    public sealed class TwitterOptions
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public bool IsConfigured()
        {
            return !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret);
        }
    }
}
