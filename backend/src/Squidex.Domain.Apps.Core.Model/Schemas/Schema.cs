// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class Schema
    {
        public SchemaType Type { get; }

        public string Name { get; }

        public string? Category { get; private set; }

        public bool IsPublished { get; private set; }

        public FieldCollection<RootField> FieldCollection { get; private set; } = FieldCollection<RootField>.Empty;

        public FieldRules FieldRules { get; private set; } = FieldRules.Empty;

        public FieldNames FieldsInLists { get; private set; } = FieldNames.Empty;

        public FieldNames FieldsInReferences { get; private set; } = FieldNames.Empty;

        public SchemaScripts Scripts { get; private set; } = SchemaScripts.Empty;

        public SchemaProperties Properties { get; private set; } = new SchemaProperties();

        public ImmutableDictionary<string, string> PreviewUrls { get; private set; } = ImmutableDictionary.Empty<string, string>();

        public IReadOnlyList<RootField> Fields
        {
            get => FieldCollection.Ordered;
        }

        public IReadOnlyDictionary<long, RootField> FieldsById
        {
            get => FieldCollection.ById;
        }

        public IReadOnlyDictionary<string, RootField> FieldsByName
        {
            get => FieldCollection.ByName;
        }

        public Schema(string name, SchemaProperties? properties = null, SchemaType type = SchemaType.Default)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            Name = name;

            if (properties != null)
            {
                Properties = properties;
            }

            Type = type;
        }

        public Schema(string name, RootField[] fields, SchemaProperties? properties, bool isPublished = false, SchemaType type = SchemaType.Default)
            : this(name, properties, type)
        {
            Guard.NotNull(fields, nameof(fields));

            FieldCollection = new FieldCollection<RootField>(fields);

            IsPublished = isPublished;
        }

        [Pure]
        public Schema Update(SchemaProperties? newProperties)
        {
            newProperties ??= new SchemaProperties();

            if (Properties.Equals(newProperties))
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.Properties = newProperties;
            });
        }

        [Pure]
        public Schema SetScripts(SchemaScripts? newScripts)
        {
            newScripts ??= new SchemaScripts();

            if (Scripts.Equals(newScripts))
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.Scripts = newScripts;
            });
        }

        [Pure]
        public Schema SetFieldsInLists(FieldNames? names)
        {
            names ??= FieldNames.Empty;

            if (FieldsInLists.SequenceEqual(names))
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.FieldsInLists = names;
            });
        }

        [Pure]
        public Schema SetFieldsInLists(params string[] names)
        {
            return SetFieldsInLists(new FieldNames(names));
        }

        [Pure]
        public Schema SetFieldsInReferences(FieldNames? names)
        {
            names ??= FieldNames.Empty;

            if (FieldsInReferences.SequenceEqual(names))
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.FieldsInReferences = names;
            });
        }

        [Pure]
        public Schema SetFieldsInReferences(params string[] names)
        {
            return SetFieldsInReferences(new FieldNames(names));
        }

        [Pure]
        public Schema SetFieldRules(FieldRules? rules)
        {
            rules ??= FieldRules.Empty;

            if (FieldRules.Equals(rules))
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.FieldRules = rules;
            });
        }

        [Pure]
        public Schema SetFieldRules(params FieldRule[] rules)
        {
            return SetFieldRules(new FieldRules(rules));
        }

        [Pure]
        public Schema Publish()
        {
            if (IsPublished)
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.IsPublished = true;
            });
        }

        [Pure]
        public Schema Unpublish()
        {
            if (!IsPublished)
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.IsPublished = false;
            });
        }

        [Pure]
        public Schema ChangeCategory(string? category)
        {
            if (string.Equals(Category, category, StringComparison.Ordinal))
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.Category = category;
            });
        }

        [Pure]
        public Schema SetPreviewUrls(ImmutableDictionary<string, string>? previewUrls)
        {
            previewUrls ??= ImmutableDictionary.Empty<string, string>();

            if (PreviewUrls.Equals(previewUrls))
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.PreviewUrls = previewUrls;
            });
        }

        [Pure]
        public Schema DeleteField(long fieldId)
        {
            if (!FieldsById.TryGetValue(fieldId, out var field))
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.FieldCollection = FieldCollection.Remove(fieldId);
                clone.FieldsInLists = FieldsInLists.Remove(field.Name);
                clone.FieldsInReferences = FieldsInReferences.Remove(field.Name);
            });
        }

        [Pure]
        public Schema ReorderFields(List<long> ids)
        {
            return UpdateFields(f => f.Reorder(ids));
        }

        [Pure]
        public Schema AddField(RootField field)
        {
            return UpdateFields(f => f.Add(field));
        }

        [Pure]
        public Schema UpdateField(long fieldId, Func<RootField, RootField> updater)
        {
            return UpdateFields(f => f.Update(fieldId, updater));
        }

        private Schema UpdateFields(Func<FieldCollection<RootField>, FieldCollection<RootField>> updater)
        {
            var newFields = updater(FieldCollection);

            if (ReferenceEquals(newFields, FieldCollection))
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.FieldCollection = newFields;
            });
        }

        private Schema Clone(Action<Schema> updater)
        {
            var clone = (Schema)MemberwiseClone();

            updater(clone);

            return clone;
        }
    }
}
