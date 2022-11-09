// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Search;

namespace Squidex.Config.Domain;

public static class SearchServices
{
    public static void AddSquidexSearch(this IServiceCollection services)
    {
        services.AddSingletonAs<SearchManager>()
            .As<ISearchManager>();
    }
}
