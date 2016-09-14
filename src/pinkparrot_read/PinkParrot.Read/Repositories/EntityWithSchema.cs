// ==========================================================================
//  EntityWithSchema.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Core.Schemas;

namespace PinkParrot.Read.Repositories
{
    public sealed class EntityWithSchema
    {
        public ISchemaEntity Entity { get; }

        public Schema Schema { get; }

        internal EntityWithSchema(ISchemaEntity entity, Schema schema)
        {
            Entity = entity;

            Schema = schema;
        }
    }
}
