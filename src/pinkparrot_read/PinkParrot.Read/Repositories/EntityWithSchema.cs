// ==========================================================================
//  EntityWithSchema.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Core.Schema;

namespace PinkParrot.Read.Repositories
{
    public sealed class EntityWithSchema
    {
        public IModelSchemaEntity Entity { get; }

        public ModelSchema Schema { get; }

        internal EntityWithSchema(IModelSchemaEntity entity, ModelSchema schema)
        {
            Entity = entity;

            Schema = schema;
        }
    }
}
