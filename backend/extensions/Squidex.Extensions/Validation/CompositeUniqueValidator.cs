// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task ValidateAsync(object value, ValidationContext context, AddError addError)
        {
            if (value is ContentData data)
            {
                var validateableFields = context.Schema.Fields.Where(IsValidateableField);

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

                    var found = await contentRepository.QueryIdsAsync(context.AppId.Id, context.SchemaId.Id, filter);

                    if (found.Any(x => x.Id != context.ContentId))
                    {
                        addError(Enumerable.Empty<string>(), "A content with the same values already exist.");
                    }
                }
            }
        }

        private static ClrValue TryGetValue(IRootField field, ContentData data)
        {
            var value = JsonValue.Null;

            if (data.TryGetValue(field.Name, out var fieldValue))
            {
                if (fieldValue.TryGetValue(InvariantPartitioning.Key, out var temp) && temp != null)
                {
                    value = temp;
                }
            }

            switch (field.RawProperties)
            {
                case BooleanFieldProperties when value is JsonBoolean boolean:
                    return boolean.Value;
                case BooleanFieldProperties when value is JsonNull:
                    return ClrValue.Null;
                case NumberFieldProperties when value is JsonNumber number:
                    return number.Value;
                case NumberFieldProperties when value is JsonNull:
                    return ClrValue.Null;
                case StringFieldProperties when value is JsonString @string:
                    return @string.Value;
                case StringFieldProperties when value is JsonNull:
                    return ClrValue.Null;
                case ReferencesFieldProperties when value is JsonArray array && array.FirstOrDefault() is JsonString @string:
                    return @string.Value;
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
