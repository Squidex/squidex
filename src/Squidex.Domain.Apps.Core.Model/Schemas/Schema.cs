// ==========================================================================
//  Schema.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class Schema
    {
        private readonly string name;
        private readonly List<Field> fieldsOrdered = new List<Field>();
        private readonly Dictionary<long, Field> fieldsById = new Dictionary<long, Field>();
        private readonly Dictionary<string, Field> fieldsByName = new Dictionary<string, Field>();
        private SchemaProperties properties = new SchemaProperties();
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

        public void Publish()
        {
            isPublished = true;
        }

        public void Unpublish()
        {
            isPublished = false;
        }

        public Schema(string name)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            this.name = name;
        }

        public void Update(SchemaProperties newProperties)
        {
            Guard.NotNull(newProperties, nameof(newProperties));

            properties = newProperties;
        }

        public void DeleteField(long fieldId)
        {
            if (!fieldsById.TryGetValue(fieldId, out var field))
            {
                return;
            }

            fieldsById.Remove(fieldId);
            fieldsByName.Remove(field.Name);
            fieldsOrdered.Remove(field);
        }

        public void ReorderFields(List<long> ids)
        {
            Guard.NotNull(ids, nameof(ids));

            if (ids.Count != fieldsOrdered.Count || ids.Any(x => !fieldsById.ContainsKey(x)))
            {
                throw new ArgumentException("Ids must cover all fields.", nameof(ids));
            }

            var fields = fieldsOrdered.ToList();

            fieldsOrdered.Clear();
            fieldsOrdered.AddRange(fields.OrderBy(f => ids.IndexOf(f.Id)));
        }

        public void AddField(Field field)
        {
            Guard.NotNull(field, nameof(field));

            if (fieldsByName.ContainsKey(field.Name) || fieldsById.ContainsKey(field.Id))
            {
                throw new ArgumentException($"A field with name '{field.Name}' already exists.", nameof(field));
            }

            fieldsById.Add(field.Id, field);
            fieldsByName.Add(field.Name, field);
            fieldsOrdered.Add(field);
        }
    }
}