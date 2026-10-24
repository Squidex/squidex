// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Queries;
using Squidex.Text;

namespace Squidex.Domain.Apps.Entities.Contents;

public class ContentSqlQueryBuilder(SqlDialect dialect, string table, SqlParams? parameters = null)
    : SqlQueryBuilder(dialect, table, parameters)
{
    public override PropertyPath Visit(PropertyPath path)
    {
        var elements = path.ToList();
        elements[0] = elements[0].ToPascalCase();

        return new PropertyPath(elements);
    }

    public override bool IsJsonPath(PropertyPath path)
    {
        if (path.Count <= 1)
        {
            return false;
        }

        // These properties are stored as JSON columns, therefore the nested fields are queried with JSON functions.
        if (string.Equals(path[0], nameof(EFContentEntity.Data), StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return string.Equals(path[0], nameof(EFContentEntity.TranslationStatus), StringComparison.OrdinalIgnoreCase);
    }
}
