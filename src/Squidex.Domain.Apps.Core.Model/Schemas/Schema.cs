// ==========================================================================
//  Schema.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class Schema : Cloneable<Schema>
    {
        private readonly string name;
        private ImmutableList<Field> fieldsOrdered = ImmutableList<Field>.Empty;
        private ImmutableDictionary<long, Field> fieldsById = ImmutableDictionary<long, Field>.Empty;
        private ImmutableDictionary<string, Field> fieldsByName = ImmutableDictionary<string, Field>.Empty;
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

        public IReadOnlyList<Field> Fields
        {
            get { return fieldsOrdered; }
        }

        public IReadOnlyDictionary<long, Field> FieldsById
        {
            get { return fieldsById; }
        }

        public IReadOnlyDictionary<string, Field> FieldsByName
        {
            get { return fieldsByName; }
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

            OnCloned();
        }

        public Schema(string name, IEnumerable<Field> fields, SchemaProperties properties, bool isPublished)
            : this(name, properties)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            this.isPublished = isPublished;

            fieldsOrdered = ImmutableList<Field>.Empty.AddRange(fields);

            OnCloned();
        }

        protected override void OnCloned()
        {
            if (fieldsOrdered.Count > 0)
            {
                fieldsById = fieldsOrdered.ToImmutableDictionary(x => x.Id);
                fieldsByName = fieldsOrdered.ToImmutableDictionary(x => x.Name);
            }
            else
            {
                fieldsById = ImmutableDictionary<long, Field>.Empty;
                fieldsByName = ImmutableDictionary<string, Field>.Empty;
            }
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
        public Schema UpdateField(long fieldId, FieldProperties newProperties)
        {
            return UpdateField(fieldId, field =>
            {
                return field.Update(newProperties);
            });
        }

        [Pure]
        public Schema LockField(long fieldId)
        {
            return UpdateField(fieldId, field =>
            {
                return field.Lock();
            });
        }

        [Pure]
        public Schema DisableField(long fieldId)
        {
            return UpdateField(fieldId, field =>
            {
                return field.Disable();
            });
        }

        [Pure]
        public Schema EnableField(long fieldId)
        {
            return UpdateField(fieldId, field =>
            {
                return field.Enable();
            });
        }

        [Pure]
        public Schema HideField(long fieldId)
        {
            return UpdateField(fieldId, field =>
            {
                return field.Hide();
            });
        }

        [Pure]
        public Schema ShowField(long fieldId)
        {
            return UpdateField(fieldId, field =>
            {
                return field.Show();
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
            if (!fieldsById.TryGetValue(fieldId, out var field))
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.fieldsOrdered = fieldsOrdered.Remove(field);
            });
        }

        [Pure]
        public Schema ReorderFields(List<long> ids)
        {
            Guard.NotNull(ids, nameof(ids));

            if (ids.Count != fieldsOrdered.Count || ids.Any(x => !fieldsById.ContainsKey(x)))
            {
                throw new ArgumentException("Ids must cover all fields.", nameof(ids));
            }

            return Clone(clone =>
            {
                clone.fieldsOrdered = fieldsOrdered.OrderBy(f => ids.IndexOf(f.Id)).ToImmutableList();
            });
        }

        [Pure]
        public Schema AddField(Field field)
        {
            Guard.NotNull(field, nameof(field));

            if (fieldsByName.ContainsKey(field.Name) || fieldsById.ContainsKey(field.Id))
            {
                throw new ArgumentException($"A field with name '{field.Name}' and id {field.Id} already exists.", nameof(field));
            }

            return Clone(clone =>
            {
                clone.fieldsOrdered = clone.fieldsOrdered.Add(field);
            });
        }

        [Pure]
        public Schema UpdateField(long fieldId, Func<Field, Field> updater)
        {
            Guard.NotNull(updater, nameof(updater));

            if (!fieldsById.TryGetValue(fieldId, out var field))
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.fieldsOrdered = clone.fieldsOrdered.Replace(field, updater(field));
            });
        }
    }
}