﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ConvertContent;

public sealed class ExcludeChangedTypes(IJsonSerializer serializer) : IContentFieldConverter, IContentValueConverter
{
    public ContentFieldData? ConvertFieldBefore(IRootField field, ContentFieldData source)
    {
        foreach (var (_, value) in source)
        {
            if (value.Value == default)
            {
                continue;
            }

            if (IsChangedType(field, value))
            {
                return null;
            }
        }

        return source;
    }

    public (bool Remove, JsonValue) ConvertValue(IField field, JsonValue source, IField? parent)
    {
        if (parent == null || source == default)
        {
            return (false, source);
        }

        return (IsChangedType(field, source), source);
    }

    private bool IsChangedType(IField field, JsonValue source)
    {
        return !JsonValueValidator.IsValid(field, source, serializer);
    }
}
