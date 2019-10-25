// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;

namespace Squidex.Domain.Apps.Entities.Rules.Repositories
{
    public class RuleStatistics
    {
        public Guid AppId { get; set; }

        public Guid RuleId { get; set; }

        public int NumSucceeded { get; set; }

        public int NumFailed { get; set; }

        public Instant? LastExecuted { get; set; }
    }
}
