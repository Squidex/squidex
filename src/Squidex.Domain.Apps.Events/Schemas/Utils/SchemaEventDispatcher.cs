// ==========================================================================
//  SchemaEventDispatcher.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Events.Schemas.Utils
{
    public static class SchemaEventDispatcher
    {
        public static Schema Dispatch(SchemaCreated @event, FieldRegistry registry)
        {
            var schema = Schema.Create(@event.Name, @event.Properties);

            if (@event.Fields != null)
            {
                var fieldId = 1;

                foreach (var eventField in @event.Fields)
                {
                    var partitioning =
                        string.Equals(eventField.Partitioning, Partitioning.Language.Key, StringComparison.OrdinalIgnoreCase) ?
                            Partitioning.Language :
                            Partitioning.Invariant;

                    var field = registry.CreateField(fieldId, eventField.Name, partitioning, eventField.Properties);

                    if (eventField.IsHidden)
                    {
                        field = field.Hide();
                    }

                    if (eventField.IsDisabled)
                    {
                        field = field.Disable();
                    }

                    if (eventField.IsLocked)
                    {
                        field = field.Lock();
                    }

                    schema = schema.AddField(field);

                    fieldId++;
                }
            }

            return schema;
        }

        public static Schema Dispatch(FieldAdded @event, Schema schema, FieldRegistry registry)
        {
            var partitioning =
                string.Equals(@event.Partitioning, Partitioning.Language.Key, StringComparison.OrdinalIgnoreCase) ?
                    Partitioning.Language :
                    Partitioning.Invariant;

            var fieldId = @event.FieldId.Id;
            var field = registry.CreateField(fieldId, @event.Name, partitioning, @event.Properties);

            if (schema.FieldsById.ContainsKey(fieldId))
            {
                return schema.UpdateField(fieldId, f => field);
            }
            else
            {
                return schema.AddField(field);
            }
        }

        public static Schema Dispatch(FieldUpdated @event, Schema schema)
        {
            return schema.UpdateField(@event.FieldId.Id, @event.Properties);
        }

        public static Schema Dispatch(FieldLocked @event, Schema schema)
        {
            return schema.LockField(@event.FieldId.Id);
        }

        public static Schema Dispatch(FieldHidden @event, Schema schema)
        {
            return schema.HideField(@event.FieldId.Id);
        }

        public static Schema Dispatch(FieldShown @event, Schema schema)
        {
            return schema.ShowField(@event.FieldId.Id);
        }

        public static Schema Dispatch(FieldDisabled @event, Schema schema)
        {
            return schema.DisableField(@event.FieldId.Id);
        }

        public static Schema Dispatch(FieldEnabled @event, Schema schema)
        {
            return schema.EnableField(@event.FieldId.Id);
        }

        public static Schema Dispatch(SchemaUpdated @event, Schema schema)
        {
            return schema.Update(@event.Properties);
        }

        public static Schema Dispatch(SchemaFieldsReordered @event, Schema schema)
        {
            return schema.ReorderFields(@event.FieldIds);
        }

        public static Schema Dispatch(FieldDeleted @event, Schema schema)
        {
            return schema.DeleteField(@event.FieldId.Id);
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
