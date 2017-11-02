// ==========================================================================
//  NoopQueryProvider.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Schemas;

namespace Squidex.Domain.Apps.Read.Contents.CustomQueries
{
    public sealed class NoopQueryProvider : ICustomQueryProvider
    {
        public Task<IReadOnlyList<ICustomQuery>> GetQueriesAsync(IAppEntity app, ISchemaEntity schema)
        {
            return Task.FromResult<IReadOnlyList<ICustomQuery>>(new List<ICustomQuery>());
        }
    }
}
