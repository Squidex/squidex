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
        private FieldCollection<NestedField> fields = FieldCollection<NestedField>.Empty;

        public IReadOnlyList<NestedField> Fields
        {
            get { return fields.Ordered; }
        }

        public IReadOnlyDictionary<long, NestedField> FieldsById
        {
            get { return fields.ById; }
        }

        public IReadOnlyDictionary<string, NestedField> FieldsByName
        {
            get { return fields.ByName; }
        }

        public ArrayField(long id, string name, Partitioning partitioning, ArrayFieldProperties properties)
            : base(id, name, partitioning, properties)
        {
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
            var newFields = updater(fields);

            if (ReferenceEquals(newFields, fields))
            {
                return this;
            }

            return Clone<ArrayField>(clone =>
            {
                clone.fields = newFields;
            });
        }
    }
}
