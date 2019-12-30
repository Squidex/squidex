// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Assets
{
    public sealed class AssetMetadata : JsonObject
    {
        public AssetMetadata SetPixelWidth(int value)
        {
            Add("pixelWidth", value);

            return this;
        }

        public AssetMetadata SetPixelHeight(int value)
        {
            Add("pixelHeight", value);

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
    }
}
