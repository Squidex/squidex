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

        public AssetMetadata SetFocusX(float value)
        {
            this["focusX"] = JsonValue.Create(value);

            return this;
        }

        public AssetMetadata SetFocusY(float value)
        {
            this["focusY"] = JsonValue.Create(value);

            return this;
        }

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

        public float? GetFocusX()
        {
            if (TryGetValue("focusX", out var n) && n is JsonNumber number)
            {
                return (float)number.Value;
            }

            return null;
        }

        public float? GetFocusY()
        {
            if (TryGetValue("focusY", out var n) && n is JsonNumber number)
            {
                return (float)number.Value;
            }

            return null;
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

        public bool TryGetNumber(string name, out double result)
        {
            if (TryGetValue(name, out var v) && v is JsonNumber n)
            {
                result = n.Value;

                return true;
            }

            result = 0;

            return false;
        }

        public bool TryGetString(string name, [MaybeNullWhen(false)] out string result)
        {
            if (TryGetValue(name, out var v) && v is JsonString s)
            {
                result = s.Value;

                return true;
            }

            result = null!;

            return false;
        }

        public bool TryGetByPath(string? path, [MaybeNullWhen(false)] out object result)
        {
            return TryGetByPath(path?.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries), out result!);
        }

        public bool TryGetByPath(IEnumerable<string>? path, [MaybeNullWhen(false)] out object result)
        {
            result = this;

            if (path == null || !path.Any())
            {
                return false;
            }

            result = null!;

            if (!TryGetValue(path.First(), out var json))
            {
                return false;
            }

            json.TryGetByPath(path.Skip(1), out var temp);

            result = temp!;

            return true;
        }
    }
}
