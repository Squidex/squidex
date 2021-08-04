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

        public const string FocusX = "focusX";
        public const string FocusY = "focusY";
        public const string PixelWidth = "pixelWidth";
        public const string PixelHeight = "pixelHeight";
        public const string VideoWidth = "videoWidth";
        public const string VideoHeight = "videoHeight";

        public AssetMetadata SetFocusX(float value)
        {
            this[FocusX] = JsonValue.Create(value);

            return this;
        }

        public AssetMetadata SetFocusY(float value)
        {
            this[FocusY] = JsonValue.Create(value);

            return this;
        }

        public AssetMetadata SetPixelWidth(int value)
        {
            this[PixelWidth] = JsonValue.Create(value);

            return this;
        }

        public AssetMetadata SetPixelHeight(int value)
        {
            this[PixelHeight] = JsonValue.Create(value);

            return this;
        }

        public AssetMetadata SetVideoWidth(int value)
        {
            this[VideoWidth] = JsonValue.Create(value);

            return this;
        }

        public AssetMetadata SetVideoHeight(int value)
        {
            this[VideoHeight] = JsonValue.Create(value);

            return this;
        }

        public float? GetFocusX()
        {
            return GetSingle(FocusX);
        }

        public float? GetFocusY()
        {
            return GetSingle(FocusY);
        }

        public int? GetPixelWidth()
        {
            return GetIn32(PixelWidth);
        }

        public int? GetPixelHeight()
        {
            return GetIn32(PixelHeight);
        }

        public int? GetVideoWidth()
        {
            return GetIn32(VideoWidth);
        }

        public int? GetVideoHeight()
        {
            return GetIn32(VideoHeight);
        }

        public int? GetIn32(string name)
        {
            if (TryGetValue(name, out var n) && n is JsonNumber number)
            {
                return (int)number.Value;
            }

            return null;
        }

        public float? GetSingle(string name)
        {
            if (TryGetValue(name, out var n) && n is JsonNumber number)
            {
                return (float)number.Value;
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
