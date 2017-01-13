// ==========================================================================
//  HideField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Write.Schemas.Commands
{
    public class HideField : SchemaAggregateCommand
    {
        public long FieldId { get; set; }
    }
}
