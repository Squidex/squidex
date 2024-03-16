// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules
{
    public sealed class RuleJob
    {
        public DomainId Id { get; set; }

        public DomainId AppId { get; set; }

        public DomainId RuleId { get; set; }

        public string EventName { get; set; }

        public string ActionName { get; set; }

        public string ActionData { get; set; }

        public string Description { get; set; }

        public long ExecutionPartition { get; set; }

        public Instant Created { get; set; }

        public Instant Expires { get; set; }
    }
}
