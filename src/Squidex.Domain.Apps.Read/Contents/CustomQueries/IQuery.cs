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
    public interface IQuery
    {
        string Name { get; }

        string Summary { get; }

        string DescriptionForSwagger { get; }

        string AssociatedToApp { get; }

        string AssociatedToSchema { get; }

        IList<QueryArgumentOption> ArgumentOptions { get; }

        Task<IReadOnlyList<IContentEntity>> Execute(ISchemaEntity schema,
            QueryContext context,
            IDictionary<string, object> arguments);
    }
}