// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities;

public sealed class BulkUpdateResult : List<BulkUpdateResultItem>
{
    public BulkUpdateResult()
    {
    }

    public BulkUpdateResult(IEnumerable<BulkUpdateResultItem> source)
        : base(source)
    {
    }
}
