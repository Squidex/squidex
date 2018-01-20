// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public static class Constants
    {
        public static readonly Duration ExpirationTime = Duration.FromDays(2);
    }
}
