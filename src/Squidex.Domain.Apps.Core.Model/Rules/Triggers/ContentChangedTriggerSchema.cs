// ==========================================================================
//  ContentChangedTriggerSchema.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Core.Rules.Triggers
{
    public sealed class ContentChangedTriggerSchema
    {
        public Guid SchemaId { get; set; }

        public bool SendCreate { get; set; }

        public bool SendUpdate { get; set; }

        public bool SendDelete { get; set; }

        public bool SendChange { get; set; }
    }
}
