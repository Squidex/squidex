// ==========================================================================
//  IQuery.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Read.Schemas;

namespace Squidex.Domain.Apps.Read.Contents.CustomQueries
{
    public interface ICustomQuery
    {
        string Name { get; }

        string Summary { get; }

        string Description { get; }

        IReadOnlyList<QueryArgumentOption> ArgumentOptions { get; }

        Task<IReadOnlyList<IContentEntity>> ExecuteAsync(ISchemaEntity schema, QueryContext context, IDictionary<string, string> arguments);
    }
}