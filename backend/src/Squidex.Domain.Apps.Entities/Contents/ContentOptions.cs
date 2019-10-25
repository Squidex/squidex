// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentOptions
    {
        public int DefaultPageSize { get; set; } = 200;

        public int DefaultPageSizeGraphQl { get; set; } = 20;

        public int MaxResults { get; set; } = 200;
    }
}
