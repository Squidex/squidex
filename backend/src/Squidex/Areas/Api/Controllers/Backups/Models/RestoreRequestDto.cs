// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Backups.Models;

[OpenApiRequest]
public sealed class RestoreRequestDto
{
    /// <summary>
    /// The name of the app.
    /// </summary>
    [LocalizedRegularExpression("^[a-z0-9]+(\\-[a-z0-9]+)*$")]
    public string? Name { get; set; }

    /// <summary>
    /// The url to the restore file.
    /// </summary>
    [LocalizedRequired]
    public Uri Url { get; set; }
}
