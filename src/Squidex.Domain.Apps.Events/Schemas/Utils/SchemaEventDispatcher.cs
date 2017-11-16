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
        public static Schema Create(SchemaCreated @event, FieldRegistry registry)
        {
            var schema = new Schema(@event.Name);

            if (@event.Properties != null)
            {
                schema = schema.Update(@event.Properties);
            }

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

        public static Schema Apply(this Schema schema, FieldAdded @event, FieldRegistry registry)
        {
            var partitioning =
                string.Equals(@event.Partitioning, Partitioning.Language.Key, StringComparison.OrdinalIgnoreCase) ?
                    Partitioning.Language :
                    Partitioning.Invariant;

            var field = registry.CreateField(@event.FieldId.Id, @event.Name, partitioning, @event.Properties);

            schema = schema.DeleteField(@event.FieldId.Id);
            schema = schema.AddField(field);

            return schema;
        }

        public static Schema Apply(this Schema schema, FieldUpdated @event)
        {
            return schema.UpdateField(@event.FieldId.Id, @event.Properties);
        }

        public static Schema Apply(this Schema schema, FieldLocked @event)
        {
            return schema.LockField(@event.FieldId.Id);
        }

        public static Schema Apply(this Schema schema, FieldHidden @event)
        {
            return schema.HideField(@event.FieldId.Id);
        }

        public static Schema Apply(this Schema schema, FieldShown @event)
        {
            return schema.ShowField(@event.FieldId.Id);
        }

        public static Schema Apply(this Schema schema, FieldDisabled @event)
        {
            return schema.DisableField(@event.FieldId.Id);
        }

        public static Schema Apply(this Schema schema, FieldEnabled @event)
        {
            return schema.EnableField(@event.FieldId.Id);
        }

        public static Schema Apply(this Schema schema, SchemaUpdated @event)
        {
            return schema.Update(@event.Properties);
        }

        public static Schema Apply(this Schema schema, SchemaFieldsReordered @event)
        {
            return schema.ReorderFields(@event.FieldIds);
        }

        public static Schema Apply(this Schema schema, FieldDeleted @event)
        {
            return schema.DeleteField(@event.FieldId.Id);
        }

        public static Schema Apply(this Schema schema, SchemaPublished @event)
        {
            return schema.Publish();
        }

        public static Schema Apply(this Schema schema, SchemaUnpublished @event)
        {
            return schema.Unpublish();
        }
    }
}
