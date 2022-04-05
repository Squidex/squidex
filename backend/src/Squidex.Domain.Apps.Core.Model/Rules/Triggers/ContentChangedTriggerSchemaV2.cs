// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules.Triggers
{
    public sealed record ContentChangedTriggerSchemaV2
    {
        public DomainId SchemaId { get; init; }

        public string? Condition { get; init; }
    }
}
