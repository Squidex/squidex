// ==========================================================================
//  ContentValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.Core.Contents;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;

#pragma warning disable 168

namespace Squidex.Core
{
    public sealed class ContentValidator
    {
        private readonly Schema schema;
        private readonly PartitionResolver partitionResolver;
        private readonly List<ValidationError> errors = new List<ValidationError>();

        public IReadOnlyList<ValidationError> Errors
        {
            get { return errors; }
        }

        public ContentValidator(Schema schema, PartitionResolver partitionResolver)
        {
            Guard.NotNull(schema, nameof(schema));
            Guard.NotNull(partitionResolver, nameof(partitionResolver));

            this.schema = schema;

            this.partitionResolver = partitionResolver;
        }

        public async Task ValidatePartialAsync(ContentData data)
        {
            Guard.NotNull(data, nameof(data));

            foreach (var fieldData in data)
            {
                var fieldName = fieldData.Key;

                if (!schema.FieldsByName.TryGetValue(fieldData.Key, out Field field))
                {
                    errors.AddError("<FIELD> is not a known field", fieldName);
                }
                else
                {
                    await ValidateFieldPartialAsync(field, fieldData.Value);
                }
            }
        }

        private async Task ValidateFieldPartialAsync(Field field, ContentFieldData fieldData)
        {
            var partitioning = field.Paritioning;
            var partition = partitionResolver(partitioning);

            foreach (var partitionValues in fieldData)
            {
                if (!partition.TryGetItem(partitionValues.Key, out var partitionItem))
                {
                    errors.AddError($"<FIELD> has an unsupported {partitioning.Key} value '{partitionValues.Key}'", field);
                }
                else
                {
                    var item = partitionItem;

                    await field.ValidateAsync(partitionValues.Value, item.IsOptional, m => errors.AddError(m, field, item));
                }
            }
        }

        public async Task ValidateAsync(ContentData data)
        {
            Guard.NotNull(data, nameof(data));

            ValidateUnknownFields(data);

            foreach (var field in schema.FieldsByName.Values)
            {
                var fieldData = data.GetOrCreate(field.Name, k => new ContentFieldData());

                await ValidateFieldAsync(field, fieldData);
            }
        }

        private void ValidateUnknownFields(ContentData data)
        {
            foreach (var fieldData in data)
            {
                if (!schema.FieldsByName.ContainsKey(fieldData.Key))
                {
                    errors.AddError("<FIELD> is not a known field", fieldData.Key);
                }
            }
        }

        private async Task ValidateFieldAsync(Field field, ContentFieldData fieldData)
        {
            var partitioning = field.Paritioning;
            var partition = partitionResolver(partitioning);

            foreach (var partitionValues in fieldData)
            {
                if (!partition.TryGetItem(partitionValues.Key, out var _))
                {
                    errors.AddError($"<FIELD> has an unsupported {partitioning.Key} value '{partitionValues.Key}'", field);
                }
            }

            foreach (var partitionItem in partition)
            {
                var value = fieldData.GetOrCreate(partitionItem.Key, k => JValue.CreateNull());

                await field.ValidateAsync(value, partitionItem.IsOptional, m => errors.AddError(m, field, partitionItem));
            }
        }
    }
}
