// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Infrastructure.Log;

[Table("Request")]
[Index(nameof(Key))]
public sealed class EFRequestEntity
{
    [Key]
    public int Id { get; set; }

    public string Key { get; set; }

    public Instant Timestamp { get; set; }

    public Dictionary<string, string> Properties { get; set; }

    public static EFRequestEntity FromRequest(Request request)
    {
        return SimpleMapper.Map(request, new EFRequestEntity());
    }

    public Request ToRequest()
    {
        return SimpleMapper.Map(this, new Request());
    }
}
