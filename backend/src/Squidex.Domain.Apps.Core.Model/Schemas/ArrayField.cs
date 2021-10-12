// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class ArrayField : RootField<ArrayFieldProperties>, IArrayField
    {
        public IReadOnlyList<NestedField> Fields
        {
            get => FieldCollection.Ordered;
        }

        public IReadOnlyDictionary<long, NestedField> FieldsById
        {
            get => FieldCollection.ById;
        }

        public IReadOnlyDictionary<string, NestedField> FieldsByName
        {
            get => FieldCollection.ByName;
        }

        public FieldCollection<NestedField> FieldCollection { get; private set;  } = FieldCollection<NestedField>.Empty;

        public ArrayField(long id, string name, Partitioning partitioning, NestedField[] fields, ArrayFieldProperties? properties = null, IFieldSettings? settings = null)
            : base(id, name, partitioning, properties, settings)
        {
            FieldCollection = new FieldCollection<NestedField>(fields);
        }

        [Pure]
        public ArrayField DeleteField(long fieldId)
        {
            return Updatefields(f => f.Remove(fieldId));
        }

        [Pure]
        public ArrayField ReorderFields(List<long> ids)
        {
            return Updatefields(f => f.Reorder(ids));
        }

        [Pure]
        public ArrayField AddField(NestedField field)
        {
            return Updatefields(f => f.Add(field));
        }

        [Pure]
        public ArrayField UpdateField(long fieldId, Func<NestedField, NestedField> updater)
        {
            return Updatefields(f => f.Update(fieldId, updater));
        }

        private ArrayField Updatefields(Func<FieldCollection<NestedField>, FieldCollection<NestedField>> updater)
        {
            var newFields = updater(FieldCollection);

            if (ReferenceEquals(newFields, FieldCollection))
            {
                return this;
            }

            return (ArrayField)Clone(clone =>
            {
                ((ArrayField)clone).FieldCollection = newFields;
            });
        }
    }
}
