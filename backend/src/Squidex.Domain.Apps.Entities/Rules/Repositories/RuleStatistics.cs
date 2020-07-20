// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules.Repositories
{
    public sealed class RuleStatistics
    {
        public DomainId AppId { get; set; }

        public DomainId RuleId { get; set; }

        public int NumSucceeded { get; set; }

        public int NumFailed { get; set; }

        public Instant? LastExecuted { get; set; }
    }
}
