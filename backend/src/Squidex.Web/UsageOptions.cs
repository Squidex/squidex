// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps.Plans;

namespace Squidex.Web
{
    public sealed class UsageOptions
    {
        public ConfigAppLimitsPlan[] Plans { get; set; }
    }
}
