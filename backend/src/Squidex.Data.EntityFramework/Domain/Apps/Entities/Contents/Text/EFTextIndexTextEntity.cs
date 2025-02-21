// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public sealed class EFTextIndexTextEntity
{
    [Key]
    required public string Id { get; set; }

    public DomainId AppId { get; set; }

    public DomainId SchemaId { get; set; }

    public DomainId ContentId { get; set; }

    public byte Stage { get; set; }

    public bool ServeAll { get; set; }

    public bool ServePublished { get; set; }

    public string Texts { get; set; }
}
