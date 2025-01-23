// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Queries;

public sealed class SqlParameters : List<object>
{
    public string AddPositional(object value)
    {
        var parameterName = $"{{{Count}}}";

        Add(value);

        return parameterName;
    }
}
