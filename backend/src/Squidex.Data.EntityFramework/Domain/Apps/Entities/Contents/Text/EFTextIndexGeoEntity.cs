// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

[Table("Geos")]
public sealed class EFTextIndexGeoEntity
{
    [Key]
    [MaxLength(400)]
    required public string Id { get; set; }

    public DomainId AppId { get; set; }

    public DomainId SchemaId { get; set; }

    public DomainId ContentId { get; set; }

    public byte Stage { get; set; }

    public bool ServeAll { get; set; }

    public bool ServePublished { get; set; }

    [MaxLength(255)]
    public string GeoField { get; set; }

    public Geometry GeoObject { get; set; }
}
