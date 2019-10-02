﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Squidex.Infrastructure;

#pragma warning disable IDE0044 // Add readonly modifier

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class FieldCollection<T> : Cloneable<FieldCollection<T>> where T : IField
    {
        public static readonly FieldCollection<T> Empty = new FieldCollection<T>();

        private static readonly Dictionary<long, T> EmptyById = new Dictionary<long, T>();
        private static readonly Dictionary<string, T> EmptyByString = new Dictionary<string, T>();

        private T[] fieldsOrdered;
        private Dictionary<long, T>? fieldsById;
        private Dictionary<string, T>? fieldsByName;

        public IReadOnlyList<T> Ordered
        {
            get { return fieldsOrdered; }
        }

        public IReadOnlyDictionary<long, T> ById
        {
            get
            {
                if (fieldsById == null)
                {
                    if (fieldsOrdered.Length == 0)
                    {
                        fieldsById = EmptyById;
                    }
                    else
                    {
                        fieldsById = fieldsOrdered.ToDictionary(x => x.Id);
                    }
                }

                return fieldsById;
            }
        }

        public IReadOnlyDictionary<string, T> ByName
        {
            get
            {
                if (fieldsByName == null)
                {
                    if (fieldsOrdered.Length == 0)
                    {
                        fieldsByName = EmptyByString;
                    }
                    else
                    {
                        fieldsByName = fieldsOrdered.ToDictionary(x => x.Name);
                    }
                }

                return fieldsByName;
            }
        }

        private FieldCollection()
        {
            fieldsOrdered = Array.Empty<T>();
        }

        public FieldCollection(T[] fields)
        {
            Guard.NotNull(fields);

            fieldsOrdered = fields;
        }

        protected override void OnCloned()
        {
            fieldsById = null;
            fieldsByName = null;
        }

        [Pure]
        public FieldCollection<T> Remove(long fieldId)
        {
            if (!ById.TryGetValue(fieldId, out _))
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.fieldsOrdered = fieldsOrdered.Where(x => x.Id != fieldId).ToArray();
            });
        }

        [Pure]
        public FieldCollection<T> Reorder(List<long> ids)
        {
            Guard.NotNull(ids);

            if (ids.Count != fieldsOrdered.Length || ids.Any(x => !ById.ContainsKey(x)))
            {
                throw new ArgumentException("Ids must cover all fields.", nameof(ids));
            }

            return Clone(clone =>
            {
                clone.fieldsOrdered = fieldsOrdered.OrderBy(f => ids.IndexOf(f.Id)).ToArray();
            });
        }

        [Pure]
        public FieldCollection<T> Add(T field)
        {
            Guard.NotNull(field);

            if (ByName.ContainsKey(field.Name))
            {
                throw new ArgumentException($"A field with name '{field.Name}' already exists.", nameof(field));
            }

            if (ById.ContainsKey(field.Id))
            {
                throw new ArgumentException($"A field with id {field.Id} already exists.", nameof(field));
            }

            return Clone(clone =>
            {
                clone.fieldsOrdered = clone.fieldsOrdered.Union(Enumerable.Repeat(field, 1)).ToArray();
            });
        }

        [Pure]
        public FieldCollection<T> Update(long fieldId, Func<T, T> updater)
        {
            Guard.NotNull(updater);

            if (!ById.TryGetValue(fieldId, out var field))
            {
                return this;
            }

            var newField = updater(field);

            if (ReferenceEquals(newField, field))
            {
                return this;
            }

            if (!(newField is T))
            {
                throw new InvalidOperationException($"Field must be of type {typeof(T)}");
            }

            return Clone(clone =>
            {
                clone.fieldsOrdered = clone.fieldsOrdered.Select(x => ReferenceEquals(x, field) ? newField : x).ToArray();
            });
        }
    }
}