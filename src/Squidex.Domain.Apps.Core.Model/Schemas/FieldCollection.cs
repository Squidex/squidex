// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class FieldCollection<T> : Cloneable<FieldCollection<T>> where T : IField
    {
        public static readonly FieldCollection<T> Empty = new FieldCollection<T>();

        private ImmutableArray<T> fieldsOrdered = ImmutableArray<T>.Empty;
        private ImmutableDictionary<long, T> fieldsById;
        private ImmutableDictionary<string, T> fieldsByName;

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
                        fieldsById = ImmutableDictionary<long, T>.Empty;
                    }
                    else
                    {
                        fieldsById = fieldsOrdered.ToImmutableDictionary(x => x.Id);
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
                        fieldsByName = ImmutableDictionary<string, T>.Empty;
                    }
                    else
                    {
                        fieldsByName = fieldsOrdered.ToImmutableDictionary(x => x.Name);
                    }
                }

                return fieldsByName;
            }
        }

        private FieldCollection()
        {
        }

        public FieldCollection(T[] fields)
        {
            Guard.NotNull(fields, nameof(fields));

            fieldsOrdered = ImmutableArray.Create(fields);
        }

        protected override void OnCloned()
        {
            fieldsById = null;
            fieldsByName = null;
        }

        [Pure]
        public FieldCollection<T> Remove(long fieldId)
        {
            if (!ById.TryGetValue(fieldId, out var field))
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.fieldsOrdered = fieldsOrdered.Remove(field);
            });
        }

        [Pure]
        public FieldCollection<T> Reorder(List<long> ids)
        {
            Guard.NotNull(ids, nameof(ids));

            if (ids.Count != fieldsOrdered.Length || ids.Any(x => !ById.ContainsKey(x)))
            {
                throw new ArgumentException("Ids must cover all fields.", nameof(ids));
            }

            return Clone(clone =>
            {
                clone.fieldsOrdered = fieldsOrdered.OrderBy(f => ids.IndexOf(f.Id)).ToImmutableArray();
            });
        }

        [Pure]
        public FieldCollection<T> Add(T field)
        {
            Guard.NotNull(field, nameof(field));

            if (ByName.ContainsKey(field.Name) || ById.ContainsKey(field.Id))
            {
                throw new ArgumentException($"A field with name '{field.Name}' and id {field.Id} already exists.", nameof(field));
            }

            return Clone(clone =>
            {
                clone.fieldsOrdered = clone.fieldsOrdered.Add(field);
            });
        }

        [Pure]
        public FieldCollection<T> Update(long fieldId, Func<T, T> updater)
        {
            Guard.NotNull(updater, nameof(updater));

            if (!ById.TryGetValue(fieldId, out var field))
            {
                return this;
            }

            var newField = updater(field);

            if (ReferenceEquals(newField, field))
            {
                return this;
            }

            if (!(newField is T typedField))
            {
                throw new InvalidOperationException($"Field must be of type {typeof(T)}");
            }

            return Clone(clone =>
            {
                clone.fieldsOrdered = clone.fieldsOrdered.Replace(field, typedField);
            });
        }
    }
}