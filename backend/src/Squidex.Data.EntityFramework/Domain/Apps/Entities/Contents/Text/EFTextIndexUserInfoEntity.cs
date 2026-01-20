// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

[Table("UserInfos")]
[Index(nameof(UserInfoApiKey))]
public sealed class EFTextIndexUserInfoEntity
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

    [MaxLength(256)]
    public string UserInfoApiKey { get; set; }

    [MaxLength(256)]
    public string UserInfoRole { get; set; }
}
