// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Queries;
using Squidex.Text;

namespace Squidex.Domain.Apps.Entities.Contents;

public class ContentQueryBuilder(SqlDialect dialect, string table, SqlParams? parameters = null) : SqlQueryBuilder(dialect, table, parameters)
{
    public override PropertyPath Visit(PropertyPath path)
    {
        var elements = path.ToList();

        elements[0] = elements[0].ToPascalCase();

        return new PropertyPath(elements);
    }

    public override bool IsJsonPath(PropertyPath path)
    {
        return path.Count > 1 && string.Equals(path[0], "data", StringComparison.OrdinalIgnoreCase);
    }
}
