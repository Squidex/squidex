// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
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
