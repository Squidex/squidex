// ==========================================================================
//  IdContentData.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.Contents
{
    public sealed class IdContentData : ContentData<long>, IEquatable<IdContentData>
    {
        public IdContentData()
            : base(EqualityComparer<long>.Default)
        {
        }

        public IdContentData(IdContentData copy)
            : base(copy, EqualityComparer<long>.Default)
        {
        }

        public IdContentData MergeInto(IdContentData target)
        {
            return Merge(this, target);
        }

        public IdContentData ToCleaned()
        {
            return Clean(this, new IdContentData());
        }

        public IdContentData AddField(long id, ContentFieldData data)
        {
            Guard.GreaterThan(id, 0, nameof(id));

            this[id] = data;

            return this;
        }

        public IdContentData ToCleanedReferences(Schema schema, ISet<Guid> deletedReferencedIds)
        {
            var result = new IdContentData(this);

            foreach (var field in schema.Fields)
            {
                if (field is IReferenceField referenceField)
                {
                    var fieldKey = GetKey(field);
                    var fieldData = this.GetOrDefault(fieldKey);

                    if (fieldData == null)
                    {
                        continue;
                    }

                    foreach (var partitionValue in fieldData.Where(x => !x.Value.IsNull()).ToList())
                    {
                        var newValue = referenceField.RemoveDeletedReferences(partitionValue.Value, deletedReferencedIds);

                        fieldData[partitionValue.Key] = newValue;
                    }
                }
            }

            return result;
        }

        public NamedContentData ToNameModel(Schema schema, bool decodeJsonField)
        {
            Guard.NotNull(schema, nameof(schema));

            var result = new NamedContentData();

            foreach (var fieldValue in this)
            {
                if (!schema.FieldsById.TryGetValue(fieldValue.Key, out var field))
                {
                    continue;
                }

                if (decodeJsonField && field is JsonField)
                {
                    var encodedValue = new ContentFieldData();

                    foreach (var partitionValue in fieldValue.Value)
                    {
                        if (partitionValue.Value.IsNull())
                        {
                            encodedValue[partitionValue.Key] = null;
                        }
                        else
                        {
                            var value = Encoding.UTF8.GetString(Convert.FromBase64String(partitionValue.Value.ToString()));

                            encodedValue[partitionValue.Key] = JToken.Parse(value);
                        }
                    }

                    result[field.Name] = encodedValue;
                }
                else
                {
                    result[field.Name] = fieldValue.Value;
                }
            }

            return result;
        }

        public bool Equals(IdContentData other)
        {
            return base.Equals(other);
        }

        public override long GetKey(Field field)
        {
            return field.Id;
        }
    }
}
