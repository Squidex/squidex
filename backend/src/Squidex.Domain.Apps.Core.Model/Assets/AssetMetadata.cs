// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Assets
{
    public sealed class AssetMetadata : Dictionary<string, IJsonValue>
    {
        private static readonly char[] PathSeparators = { '.', '[', ']' };

        public AssetMetadata SetPixelWidth(int value)
        {
            this["pixelWidth"] = JsonValue.Create(value);

            return this;
        }

        public AssetMetadata SetPixelHeight(int value)
        {
            this["pixelHeight"] = JsonValue.Create(value);

            return this;
        }

        public int? GetPixelWidth()
        {
            if (TryGetValue("pixelWidth", out var n) && n is JsonNumber number)
            {
                return (int)number.Value;
            }

            return null;
        }

        public int? GetPixelHeight()
        {
            if (TryGetValue("pixelHeight", out var n) && n is JsonNumber number)
            {
                return (int)number.Value;
            }

            return null;
        }

        public bool TryGetByPath(string? path, [MaybeNullWhen(false)] out object result)
        {
            return TryGetByPath(path?.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries), out result!);
        }

        public bool TryGetByPath(IEnumerable<string>? path, [MaybeNullWhen(false)] out object result)
        {
            if (path == null || !path.Any())
            {
                result = this;
                return false;
            }

            if (!TryGetValue(path.First(), out var json))
            {
                result = null!;
                return false;
            }

            foreach (var pathSegment in path.Skip(1))
            {
                if (json == null || !json.TryGet(pathSegment, out json))
                {
                    result = null!;
                    return false;
                }
            }

            result = json;

            return true;
        }
    }
}
