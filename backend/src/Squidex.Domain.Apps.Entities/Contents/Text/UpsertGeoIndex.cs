// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using GeoJSON.Net;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class UpsertGeoIndex : UpdateIndexEntry
    {
        public Dictionary<string, GeoJSONObject>? GeoObjects { get; set; }

        public DomainId ContentId { get; set; }

        public bool IsNew { get; set; }
    }
}
