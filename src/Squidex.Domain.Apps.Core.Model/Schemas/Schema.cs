// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class Schema : Cloneable<Schema>
    {
        private readonly string name;
        private FieldCollection<RootField> fields = FieldCollection<RootField>.Empty;
        private SchemaProperties properties;
        private bool isPublished;

        public string Name
        {
            get { return name; }
        }

        public bool IsPublished
        {
            get { return isPublished; }
        }

        public IReadOnlyList<RootField> Fields
        {
            get { return fields.Ordered; }
        }

        public IReadOnlyDictionary<long, RootField> FieldsById
        {
            get { return fields.ById; }
        }

        public IReadOnlyDictionary<string, RootField> FieldsByName
        {
            get { return fields.ByName; }
        }

        public SchemaProperties Properties
        {
            get { return properties; }
        }

        public Schema(string name, SchemaProperties properties = null)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            this.name = name;

            this.properties = properties ?? new SchemaProperties();
            this.properties.Freeze();
        }

        public Schema(string name, RootField[] fields, SchemaProperties properties, bool isPublished)
            : this(name, properties)
        {
            Guard.NotNull(fields, nameof(fields));

            this.fields = new FieldCollection<RootField>(fields);

            this.isPublished = isPublished;
        }

        [Pure]
        public Schema Update(SchemaProperties newProperties)
        {
            Guard.NotNull(newProperties, nameof(newProperties));

            return Clone(clone =>
            {
                clone.properties = newProperties;
                clone.properties.Freeze();
            });
        }

        [Pure]
        public Schema Publish()
        {
            return Clone(clone =>
            {
                clone.isPublished = true;
            });
        }

        [Pure]
        public Schema Unpublish()
        {
            return Clone(clone =>
            {
                clone.isPublished = false;
            });
        }

        [Pure]
        public Schema DeleteField(long fieldId)
        {
            return Updatefields(f => f.Remove(fieldId));
        }

        [Pure]
        public Schema ReorderFields(List<long> ids)
        {
            return Updatefields(f => f.Reorder(ids));
        }

        [Pure]
        public Schema AddField(RootField field)
        {
            return Updatefields(f => f.Add(field));
        }

        [Pure]
        public Schema UpdateField(long fieldId, Func<RootField, RootField> updater)
        {
            return Updatefields(f => f.Update(fieldId, updater));
        }

        private Schema Updatefields(Func<FieldCollection<RootField>, FieldCollection<RootField>> updater)
        {
            var newFields = updater(fields);

            if (ReferenceEquals(newFields, fields))
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.fields = newFields;
            });
        }
    }
}