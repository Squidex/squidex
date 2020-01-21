// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ImportResultItem
    {
        public Guid? ContentId { get; set; }

        public Exception? Exception { get; set; }
    }
}
