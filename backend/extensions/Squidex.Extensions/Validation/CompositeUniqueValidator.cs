// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Queries;

namespace Squidex.Extensions.Validation
{
    internal sealed class CompositeUniqueValidator : IValidator
    {
        private readonly string tag;
        private readonly IContentRepository contentRepository;

        public CompositeUniqueValidator(string tag, IContentRepository contentRepository)
        {
            this.tag = tag;

            this.contentRepository = contentRepository;
        }

        public void Validate(object value, ValidationContext context)
        {
            if (value is ContentData data)
            {
                context.Root.AddTask(async ct => await ValidateAsync(data, context));
            }
        }

        private async Task ValidateAsync(ContentData data, ValidationContext context)
        {
            var validateableFields = context.Root.Schema.Fields.Where(IsValidateableField);

            var filters = new List<FilterNode<ClrValue>>();

            foreach (var field in validateableFields)
            {
                var fieldValue = TryGetValue(field, data);

                if (fieldValue != null)
                {
                    filters.Add(ClrFilter.Eq($"data.{field.Name}.iv", fieldValue));
                }
            }

            if (filters.Count > 0)
            {
                var filter = ClrFilter.And(filters);

                var found = await contentRepository.QueryIdsAsync(context.Root.AppId.Id, context.Root.SchemaId.Id, filter);

                if (found.Any(x => x.Id != context.Root.ContentId))
                {
                    context.AddError(Enumerable.Empty<string>(), "A content with the same values already exist.");
                }
            }
        }

        private static ClrValue TryGetValue(IRootField field, ContentData data)
        {
            var value = JsonValue.Null;

            if (data.TryGetValue(field.Name, out var fieldValue))
            {
                if (fieldValue.TryGetValue(InvariantPartitioning.Key, out var temp) && temp != default)
                {
                    value = temp;
                }
            }

            switch (field.RawProperties)
            {
                case BooleanFieldProperties when value.Type == JsonValueType.Boolean:
                    return value.AsBoolean;
                case BooleanFieldProperties when value.Type == JsonValueType.Null:
                    return ClrValue.Null;
                case NumberFieldProperties when value.Type == JsonValueType.Number:
                    return value.AsNumber;
                case NumberFieldProperties when value.Type == JsonValueType.Null:
                    return ClrValue.Null;
                case StringFieldProperties when value.Type == JsonValueType.String:
                    return value.AsString;
                case StringFieldProperties when value.Type == JsonValueType.Null:
                    return ClrValue.Null;
                case ReferencesFieldProperties when value.Type == JsonValueType.Array:
                    var first = value.AsArray.FirstOrDefault();

                    if (first.Type == JsonValueType.String)
                    {
                        return first.AsString;
                    }

                    break;
            }

            return null;
        }

        private bool IsValidateableField(IRootField field)
        {
            return
                field.Partitioning == Partitioning.Invariant &&
                field.RawProperties.Tags?.Contains(tag) == true &&
                (field.RawProperties is BooleanFieldProperties ||
                 field.RawProperties is NumberFieldProperties ||
                 field.RawProperties is ReferencesFieldProperties ||
                 field.RawProperties is StringFieldProperties);
        }
    }
}
