// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Squidex.Infrastructure.Caching;

[Table("Cache")]
public class EFCacheEntity
{
    [Key]
    public string Key { get; set; }

    public DateTime Expires { get; set; }

    public byte[] Value { get; set; }
}
