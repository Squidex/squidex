// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.Api.Controllers.Statistics.Models;

public sealed class CurrentStorageDto
{
    /// <summary>
    /// The size in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// The maximum allowed asset size.
    /// </summary>
    public long MaxAllowed { get; set; }
}
