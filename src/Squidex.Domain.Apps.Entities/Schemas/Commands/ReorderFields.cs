// ==========================================================================
//  ReorderFields.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public sealed class ReorderFields : SchemaAggregateCommand
    {
        public List<long> FieldIds { get; set; }
    }
}
