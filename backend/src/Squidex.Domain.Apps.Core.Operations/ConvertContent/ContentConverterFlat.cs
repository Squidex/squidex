﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ConvertContent
{
    public static class ContentConverterFlat
    {
        public static Dictionary<string, object?> ToFlatten(this NamedContentData content)
        {
            var result = new Dictionary<string, object?>();

            foreach (var (key, value) in content)
            {
                result[key] = GetFirst(value);
            }

            return result;
        }

        public static FlatContentData ToFlatten(this NamedContentData content, string fallback)
        {
            var result = new FlatContentData();

            foreach (var (key, value) in content)
            {
                result[key] = GetFirst(value, fallback);
            }

            return result;
        }

        private static object? GetFirst(ContentFieldData? fieldData)
        {
            if (fieldData == null)
            {
                return null;
            }

            if (fieldData.Count == 1)
            {
                return fieldData.Values.First();
            }

            return fieldData;
        }

        private static IJsonValue? GetFirst(ContentFieldData? fieldData, string fallback)
        {
            if (fieldData == null)
            {
                return null;
            }

            if (fieldData.Count == 1)
            {
                return fieldData.Values.First();
            }

            if (fieldData.TryGetValue(fallback, out var value))
            {
                return value;
            }

            if (fieldData.Count > 1)
            {
                return fieldData.Values.First();
            }

            return null;
        }
    }
}
