// ==========================================================================
//  UpdateSchema.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Write.Schemas.Commands
{
    public sealed class UpdateSchema : SchemaAggregateCommand
    {
        public SchemaProperties Properties { get; set; }
    }
}