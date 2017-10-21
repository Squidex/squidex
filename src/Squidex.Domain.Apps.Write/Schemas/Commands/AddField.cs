// ==========================================================================
//  AddField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Write.Schemas.Commands
{
    public sealed class AddField : SchemaAggregateCommand
    {
        public string Name { get; set; }

        public string Partitioning { get; set; }

        public FieldProperties Properties { get; set; }
    }
}