// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Schemas
{
    [EventType(nameof(FieldAdded))]
    public sealed class FieldAdded : FieldEvent
    {
        public string Name { get; set; }

        public string? Partitioning { get; set; }

        public FieldProperties Properties { get; set; }
    }
}