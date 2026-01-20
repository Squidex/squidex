// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NetTopologySuite.Geometries;
using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public sealed class UpsertIndexEntry : IndexCommand
{
    public Dictionary<string, Geometry>? GeoObjects { get; set; }

    public Dictionary<string, string>? Texts { get; set; }

    public List<UserInfoValue>? UserInfos { get; set; }

    public bool ServeAll { get; set; }

    public bool ServePublished { get; set; }

    public bool IsNew { get; set; }
}
