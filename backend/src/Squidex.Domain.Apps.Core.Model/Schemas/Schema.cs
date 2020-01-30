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
        private static readonly Dictionary<string, string> EmptyPreviewUrls = new Dictionary<string, string>();
        private readonly string name;
        private readonly bool isSingleton;
        private string category;
        private FieldNames fieldsInLists = FieldNames.Empty;
        private FieldNames fieldsInReferences = FieldNames.Empty;
        private FieldCollection<RootField> fields = FieldCollection<RootField>.Empty;
        private IReadOnlyDictionary<string, string> previewUrls = EmptyPreviewUrls;
        private SchemaScripts scripts = SchemaScripts.Empty;
        private SchemaProperties properties;
        private bool isPublished;

        public string Name
        {
            get { return name; }
        }

        public string Category
        {
            get { return category; }
        }

        public bool IsPublished
        {
            get { return isPublished; }
        }

        public bool IsSingleton
        {
            get { return isSingleton; }
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

        public IReadOnlyDictionary<string, string> PreviewUrls
        {
            get { return previewUrls; }
        }

        public FieldCollection<RootField> FieldCollection
        {
            get { return fields; }
        }

        public FieldNames FieldsInLists
        {
            get { return fieldsInLists; }
        }

        public FieldNames FieldsInReferences
        {
            get { return fieldsInReferences; }
        }

        public SchemaScripts Scripts
        {
            get { return scripts; }
        }

        public SchemaProperties Properties
        {
            get { return properties; }
        }

        public Schema(string name, SchemaProperties? properties = null, bool isSingleton = false)
        {
            Guard.NotNullOrEmpty(name);

            this.name = name;

            this.properties = properties ?? new SchemaProperties();
            this.properties.Freeze();

            this.isSingleton = isSingleton;
        }

        public Schema(string name, RootField[] fields, SchemaProperties properties, bool isPublished, bool isSingleton = false)
            : this(name, properties, isSingleton)
        {
            Guard.NotNull(fields);

            this.fields = new FieldCollection<RootField>(fields);

            this.isPublished = isPublished;
        }

        [Pure]
        public Schema Update(SchemaProperties newProperties)
        {
            newProperties ??= new SchemaProperties();

            if (properties.DeepEquals(newProperties))
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.properties = newProperties;
                clone.Properties.Freeze();
            });
        }

        [Pure]
        public Schema ConfigureScripts(SchemaScripts newScripts)
        {
            newScripts ??= new SchemaScripts();

            if (scripts.Equals(newScripts))
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.scripts = newScripts;
                clone.scripts.Freeze();
            });
        }

        [Pure]
        public Schema ConfigureFieldsInLists(FieldNames names)
        {
            names ??= FieldNames.Empty;

            if (fieldsInLists.SetEquals(names))
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.fieldsInLists = names;
            });
        }

        [Pure]
        public Schema ConfigureFieldsInLists(params string[] names)
        {
            return ConfigureFieldsInLists(new FieldNames(names));
        }

        [Pure]
        public Schema ConfigureFieldsInReferences(FieldNames names)
        {
            names ??= FieldNames.Empty;

            if (fieldsInReferences.SetEquals(names))
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.fieldsInReferences = names;
            });
        }

        [Pure]
        public Schema ConfigureFieldsInReferences(params string[] names)
        {
            return ConfigureFieldsInReferences(new FieldNames(names));
        }

        [Pure]
        public Schema Publish()
        {
            if (isPublished)
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.isPublished = true;
            });
        }

        [Pure]
        public Schema Unpublish()
        {
            if (!isPublished)
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.isPublished = false;
            });
        }

        [Pure]
        public Schema ChangeCategory(string newCategory)
        {
            if (string.Equals(category, newCategory))
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.category = newCategory;
            });
        }

        [Pure]
        public Schema ConfigurePreviewUrls(IReadOnlyDictionary<string, string> newPreviewUrls)
        {
            previewUrls ??= EmptyPreviewUrls;

            if (previewUrls.EqualsDictionary(newPreviewUrls))
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.previewUrls = newPreviewUrls;
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
                clone.fields = fields.Remove(fieldId);
                clone.fieldsInLists = fieldsInLists.Remove(field.Name);
                clone.fieldsInReferences = fieldsInReferences.Remove(field.Name);
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