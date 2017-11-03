// ==========================================================================
//  ICustomQuery.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Squidex.Domain.Apps.Read.Contents.CustomQueries
{
    public interface ICustomQuery
    {
        string Name { get; }

        string Summary { get; }

        string Description { get; }

        IReadOnlyList<QueryArgumentOption> ArgumentOptions { get; }

        Task<JToken> ExecuteAsync(QueryContext context, IDictionary<string, string> arguments);
    }
}