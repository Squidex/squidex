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
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Events.Schemas.Utils
{
    public static class SchemaEventDispatcher
    {
        public static Schema Create(SchemaCreated @event, FieldRegistry registry)
        {
            var schema = new Schema(@event.Name);

            if (@event.Properties != null)
            {
                schema.Update(@event.Properties);
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
                        field.Hide();
                    }

                    if (eventField.IsDisabled)
                    {
                        field.Disable();
                    }

                    if (eventField.IsLocked)
                    {
                        field.Lock();
                    }

                    schema.AddField(field);

                    fieldId++;
                }
            }

            return schema;
        }

        public static void Apply(this Schema schema, FieldAdded @event, FieldRegistry registry)
        {
            var partitioning =
                string.Equals(@event.Partitioning, Partitioning.Language.Key, StringComparison.OrdinalIgnoreCase) ?
                    Partitioning.Language :
                    Partitioning.Invariant;

            var fieldId = @event.FieldId.Id;
            var field = registry.CreateField(fieldId, @event.Name, partitioning, @event.Properties);

            schema.DeleteField(fieldId);
            schema.AddField(field);
        }

        public static void Apply(this Schema schema, FieldUpdated @event)
        {
            if (schema.FieldsById.TryGetValue(@event.FieldId.Id, out var field))
            {
                field.Update(@event.Properties);
            }
        }

        public static void Apply(this Schema schema, FieldLocked @event)
        {
            if (schema.FieldsById.TryGetValue(@event.FieldId.Id, out var field))
            {
                field.Lock();
            }
        }

        public static void Apply(this Schema schema, FieldHidden @event)
        {
            if (schema.FieldsById.TryGetValue(@event.FieldId.Id, out var field))
            {
                field.Hide();
            }
        }

        public static void Apply(this Schema schema, FieldShown @event)
        {
            if (schema.FieldsById.TryGetValue(@event.FieldId.Id, out var field))
            {
                field.Show();
            }
        }

        public static void Apply(this Schema schema, FieldDisabled @event)
        {
            if (schema.FieldsById.TryGetValue(@event.FieldId.Id, out var field))
            {
                field.Disable();
            }
        }

        public static void Apply(this Schema schema, FieldEnabled @event)
        {
            if (schema.FieldsById.TryGetValue(@event.FieldId.Id, out var field))
            {
                field.Enable();
            }
        }

        public static void Apply(this Schema schema, SchemaUpdated @event)
        {
            schema.Update(@event.Properties);
        }

        public static void Apply(this Schema schema, SchemaFieldsReordered @event)
        {
            schema.ReorderFields(@event.FieldIds);
        }

        public static void Apply(this Schema schema, FieldDeleted @event)
        {
            schema.DeleteField(@event.FieldId.Id);
        }

        public static void Apply(this Schema schema, SchemaPublished @event)
        {
            schema.Publish();
        }

        public static void Apply(this Schema schema, SchemaUnpublished @event)
        {
            schema.Unpublish();
        }

        public static void Apply(this Schema schema, ScriptsConfigured @event)
        {
            SimpleMapper.Map(@event, schema);
        }
    }
}
