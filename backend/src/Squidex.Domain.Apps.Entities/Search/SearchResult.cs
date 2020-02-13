// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Search
{
    public sealed class SearchResult
    {
        public string Name { get; set; }

        public string? Label { get; set; }

        public string Url { get; set; }

        public SearchResultType Type { get; set; }
    }
}