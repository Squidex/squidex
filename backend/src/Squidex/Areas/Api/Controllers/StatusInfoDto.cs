// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Areas.Api.Controllers;

public sealed class StatusInfoDto
{
    /// <summary>
    /// The name of the status.
    /// </summary>
    public Status Status { get; set; }

    /// <summary>
    /// The color of the status.
    /// </summary>
    public string Color { get; set; }

    public static StatusInfoDto FromDomain(StatusInfo statusInfo)
    {
        var result = new StatusInfoDto { Status = statusInfo.Status, Color = statusInfo.Color };

        return result;
    }
}
