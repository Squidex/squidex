// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Tags
{
    public sealed class Tag
    {
        public string Name { get; set; }

        public int Count { get; set; } = 1;
    }
}
