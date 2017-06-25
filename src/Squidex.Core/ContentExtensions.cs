// ==========================================================================
//  ContentExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Core.Contents;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;

// ReSharper disable InvertIf

namespace Squidex.Core
{
    public static class ContentExtensions
    {
        public static ContentData Enrich(this ContentData data, Schema schema, PartitionResolver partitionResolver)
        {
            var enricher = new ContentEnricher(schema, partitionResolver);

            enricher.Enrich(data);

            return data;
        }

        public static async Task ValidateAsync(this ContentData data, Schema schema, PartitionResolver partitionResolver, IList<ValidationError> errors)
        {
            var validator = new ContentValidator(schema, partitionResolver);

            await validator.ValidateAsync(data);

            foreach (var error in validator.Errors)
            {
                errors.Add(error);
            }
        }

        public static async Task ValidatePartialAsync(this ContentData data, Schema schema, PartitionResolver partitionResolver, IList<ValidationError> errors)
        {
            var validator = new ContentValidator(schema, partitionResolver);

            await validator.ValidatePartialAsync(data);

            foreach (var error in validator.Errors)
            {
                errors.Add(error);
            }
        }

        public static IEnumerable<Guid> GetReferencedIds(this ContentData data, Schema schema)
        {
            var foundReferences = new HashSet<Guid>();

            foreach (var field in schema.Fields)
            {
                if (field is IReferenceField referenceField)
                {
                    var fieldData = data.GetOrDefault(field.Id.ToString());

                    if (fieldData == null)
                    {
                        continue;
                    }

                    foreach (var partitionValue in fieldData.Where(x => x.Value != null))
                    {
                        var ids = referenceField.GetReferencedIds(partitionValue.Value);

                        foreach (var id in ids.Where(x => foundReferences.Add(x)))
                        {
                            yield return id;
                        }
                    }
                }
            }
        }

        public static ContentData ToCleanedReferences(this ContentData data, Schema schema, ISet<Guid> deletedReferencedIds)
        {
            var result = new ContentData(data);

            foreach (var field in schema.Fields)
            {
                if (field is IReferenceField referenceField)
                {
                    var fieldData = data.GetOrDefault(field.Id.ToString());

                    if (fieldData == null)
                    {
                        continue;
                    }

                    foreach (var partitionValue in fieldData.Where(x => x.Value != null).ToList())
                    {
                        var newValue = referenceField.RemoveDeletedReferences(partitionValue.Value, deletedReferencedIds);

                        fieldData[partitionValue.Key] = newValue;
                    }
                }
            }

            return result;
        }
    }
}
