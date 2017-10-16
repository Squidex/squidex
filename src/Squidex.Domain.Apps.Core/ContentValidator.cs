// ==========================================================================
//  ContentValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

#pragma warning disable 168

namespace Squidex.Domain.Apps.Core
{
    public sealed class ContentValidator
    {
        private readonly Schema schema;
        private readonly PartitionResolver partitionResolver;
        private readonly ValidationContext context;
        private readonly ConcurrentBag<ValidationError> errors = new ConcurrentBag<ValidationError>();

        public IReadOnlyCollection<ValidationError> Errors
        {
            get { return errors; }
        }

        public ContentValidator(Schema schema, PartitionResolver partitionResolver, ValidationContext context)
        {
            Guard.NotNull(schema, nameof(schema));
            Guard.NotNull(partitionResolver, nameof(partitionResolver));

            this.schema = schema;
            this.context = context;
            this.partitionResolver = partitionResolver;
        }

        public Task ValidatePartialAsync(NamedContentData data)
        {
            Guard.NotNull(data, nameof(data));

            var tasks = new List<Task>();

            foreach (var fieldData in data)
            {
                var fieldName = fieldData.Key;

                if (!schema.FieldsByName.TryGetValue(fieldData.Key, out var field))
                {
                    errors.AddError("<FIELD> is not a known field.", fieldName);
                }
                else
                {
                    tasks.Add(ValidateFieldPartialAsync(field, fieldData.Value));
                }
            }

            return Task.WhenAll(tasks);
        }

        private Task ValidateFieldPartialAsync(Field field, ContentFieldData fieldData)
        {
            var partitioning = field.Partitioning;
            var partition = partitionResolver(partitioning);

            var tasks = new List<Task>();

            foreach (var partitionValues in fieldData)
            {
                if (partition.TryGetItem(partitionValues.Key, out var item))
                {
                    tasks.Add(field.ValidateAsync(partitionValues.Value, context.Optional(item.IsOptional), m => errors.AddError(m, field, item)));
                }
                else
                {
                    errors.AddError($"<FIELD> has an unsupported {partitioning.Key} value '{partitionValues.Key}'.", field);
                }
            }

            return Task.WhenAll(tasks);
        }

        public Task ValidateAsync(NamedContentData data)
        {
            Guard.NotNull(data, nameof(data));

            ValidateUnknownFields(data);

            var tasks = new List<Task>();

            foreach (var field in schema.FieldsByName.Values)
            {
                var fieldData = data.GetOrCreate(field.Name, k => new ContentFieldData());

                tasks.Add(ValidateFieldAsync(field, fieldData));
            }

            return Task.WhenAll(tasks);
        }

        private void ValidateUnknownFields(NamedContentData data)
        {
            foreach (var fieldData in data)
            {
                if (!schema.FieldsByName.ContainsKey(fieldData.Key))
                {
                    errors.AddError("<FIELD> is not a known field.", fieldData.Key);
                }
            }
        }

        private Task ValidateFieldAsync(Field field, ContentFieldData fieldData)
        {
            var partitioning = field.Partitioning;
            var partition = partitionResolver(partitioning);

            var tasks = new List<Task>();

            foreach (var partitionValues in fieldData)
            {
                if (!partition.TryGetItem(partitionValues.Key, out var _))
                {
                    errors.AddError($"<FIELD> has an unsupported {partitioning.Key} value '{partitionValues.Key}'.", field);
                }
            }

            foreach (var item in partition)
            {
                var value = fieldData.GetOrCreate(item.Key, k => JValue.CreateNull());

                tasks.Add(field.ValidateAsync(value, context.Optional(item.IsOptional), m => errors.AddError(m, field, item)));
            }

            return Task.WhenAll(tasks);
        }
    }
}
