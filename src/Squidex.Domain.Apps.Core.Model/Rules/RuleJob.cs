// ==========================================================================
//  RuleJob.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using NodaTime;

namespace Squidex.Domain.Apps.Core.Rules
{
    public sealed class RuleJob
    {
        public Guid Id { get; set; }

        public Guid AppId { get; set; }

        public string EventName { get; set; }

        public string ActionName { get; set; }

        public string Description { get; set; }

        public Instant Created { get; set; }

        public Instant Expires { get; set; }

        public RuleJobData ActionData { get; set; }
    }
}
