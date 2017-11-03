// ==========================================================================
//  IQueryProvider.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Read.Apps;

namespace Squidex.Domain.Apps.Read.Contents.CustomQueries
{
    public interface ICustomQueryProvider
    {
        Task<IReadOnlyList<ICustomQuery>> GetQueriesAsync(IAppEntity app);
    }
}
