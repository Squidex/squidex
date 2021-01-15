﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GeoJSON.Net.Converters;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class WriteonlyGeoJsonConverter : GeoJsonConverter
    {
        public override bool CanWrite => false;
    }
}
