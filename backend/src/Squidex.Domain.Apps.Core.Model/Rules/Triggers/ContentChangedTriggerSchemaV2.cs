// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules.Triggers
{
    public sealed class ContentChangedTriggerSchemaV2 : Freezable
    {
        public DomainId SchemaId { get; set; }

        public string? Condition { get; set; }
    }
}
