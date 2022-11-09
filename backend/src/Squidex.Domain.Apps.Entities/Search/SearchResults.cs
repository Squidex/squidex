// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Search;

public sealed class SearchResults : List<SearchResult>
{
    public SearchResults()
    {
    }

    public SearchResults(IEnumerable<SearchResult> source)
        : base(source)
    {
    }

    public SearchResults Add(string name, SearchResultType type, string url, string? label = null)
    {
        Add(new SearchResult { Name = name, Type = type, Label = label, Url = url });

        return this;
    }
}
