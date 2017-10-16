// ==========================================================================
//  IQueryProvider.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
using System.Collections.Generic;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Schemas;

namespace Squidex.Domain.Apps.Read.Contents.CustomQueries
{
    public interface IQueryProvider
    {
        IEnumerable<IQuery> GetQueries(IAppEntity app, ISchemaEntity schema);
    }
}
