// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Entities.Contents;

internal static class Extensions
{
    public static bool ShouldWritePublished(this WriteContent content)
    {
        return content.CurrentVersion.Status == Status.Published && !content.IsDeleted;
    }
}
