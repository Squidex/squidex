// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class BulkUpdateResultItem
    {
        public DomainId? ContentId { get; set; }

        public int JobIndex { get; set; }

        public Exception? Exception { get; set; }
    }
}
