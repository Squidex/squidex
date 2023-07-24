// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Entities.Contents;

public static class ContentExtensions
{
    public static Status EditingStatus(this IContentEntity content)
    {
        return content.NewStatus ?? content.Status;
    }
}
