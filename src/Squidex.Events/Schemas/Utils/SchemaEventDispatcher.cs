// ==========================================================================
//  SchemaUpdater.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Core.Schemas;

namespace Squidex.Events.Schemas.Utils
{
    public class SchemaEventDispatcher
    {
        public static Schema Dispatch(SchemaCreated @event)
        {
            return Schema.Create(@event.Name, @event.Properties);
        }

        public static Schema Dispatch(FieldAdded @event, Schema schema, FieldRegistry registry)
        {
            return schema.AddOrUpdateField(registry.CreateField(@event.FieldId, @event.Name, @event.Properties));
        }

        public static Schema Dispatch(FieldUpdated @event, Schema schema)
        {
            return schema.UpdateField(@event.FieldId, @event.Properties);
        }

        public static Schema Dispatch(FieldHidden @event, Schema schema)
        {
            return schema.HideField(@event.FieldId);
        }

        public static Schema Dispatch(FieldShown @event, Schema schema)
        {
            return schema.ShowField(@event.FieldId);
        }

        public static Schema Dispatch(FieldDisabled @event, Schema schema)
        {
            return schema.DisableField(@event.FieldId);
        }

        public static Schema Dispatch(FieldEnabled @event, Schema schema)
        {
            return schema.EnableField(@event.FieldId);
        }

        public static Schema Dispatch(SchemaUpdated @event, Schema schema)
        {
            return schema.Update(@event.Properties);
        }

        public static Schema Dispatch(FieldDeleted @event, Schema schema)
        {
            return schema.DeleteField(@event.FieldId);
        }

        public static Schema Dispatch(SchemaPublished @event, Schema schema)
        {
            return schema.Publish();
        }

        public static Schema Dispatch(SchemaUnpublished @event, Schema schema)
        {
            return schema.Unpublish();
        }
    }
}
