// ==========================================================================
//  FieldCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public class FieldCommand : SchemaAggregateCommand
    {
        public long FieldId { get; set; }
    }
}
