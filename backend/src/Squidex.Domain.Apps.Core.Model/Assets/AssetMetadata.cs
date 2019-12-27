// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Squidex.Domain.Apps.Core.Assets
{
    public sealed class AssetMetadata : Dictionary<string, string>
    {
        public AssetMetadata()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public AssetMetadata SetPixelWidth(int value)
        {
            this["pixelWidth"] = value.ToString(CultureInfo.InvariantCulture);

            return this;
        }

        public AssetMetadata SetPixelHeight(int value)
        {
            this["pixelHeight"] = value.ToString(CultureInfo.InvariantCulture);

            return this;
        }

        public int GetPixelWidth()
        {
            if (TryGetValue("pixelWidth", out var w) && int.TryParse(w, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            return 0;
        }

        public int GetPixelHeight()
        {
            if (TryGetValue("pixelHeight", out var w) && int.TryParse(w, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            return 0;
        }
    }
}
