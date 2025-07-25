﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Queries;

namespace Squidex.Extensions.Validation;

internal sealed class CompositeUniqueValidator(string contentTag, IContentRepository contentRepository) : IValidator
{
    public void Validate(object? value, ValidationContext context)
    {
        if (value is ContentData data)
        {
            context.Root.AddTask(ct => ValidateAsync(data, context));
        }
    }

    private async ValueTask ValidateAsync(ContentData data, ValidationContext context)
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

            var found = await contentRepository.QueryIdsAsync(context.Root.App, context.Root.Schema, filter, SearchScope.All);
            if (found.Any(x => x.Id != context.Root.ContentId))
            {
                context.AddError("A content with the same values already exist.", []);
            }
        }
    }

    private static ClrValue? TryGetValue(RootField field, ContentData data)
    {
        var value = JsonValue.Null;

        if (data.TryGetValue(field.Name, out var fieldValue) && fieldValue != null)
        {
            if (fieldValue.TryGetValue(InvariantPartitioning.Key, out var temp) && temp != default)
            {
                value = temp;
            }
        }

        switch (field.RawProperties)
        {
            case BooleanFieldProperties when value.Value is bool b:
                return b;
            case BooleanFieldProperties when value.Value == default:
                return ClrValue.Null;
            case NumberFieldProperties when value.Value is double n:
                return n;
            case NumberFieldProperties when value.Value == default:
                return ClrValue.Null;
            case StringFieldProperties when value.Value is string s:
                return s;
            case StringFieldProperties when value.Value == default:
                return ClrValue.Null;
            case ReferencesFieldProperties when value.Value is JsonArray a:
                if (a.FirstOrDefault().Value is string first)
                {
                    return first;
                }

                break;
        }

        return null;
    }

    private bool IsValidateableField(IRootField field)
    {
        return
            field.Partitioning == Partitioning.Invariant &&
            field.RawProperties.Tags?.Contains(contentTag) == true &&
            field.RawProperties is BooleanFieldProperties or DateTimeFieldProperties or NumberFieldProperties or ReferencesFieldProperties or StringFieldProperties;
    }
}
