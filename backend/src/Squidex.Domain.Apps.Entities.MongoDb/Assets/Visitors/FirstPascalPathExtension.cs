// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Queries;
using Squidex.Text;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets.Visitors;

public static class FirstPascalPathExtension
{
    public static PropertyPath ToFirstPascalCase(this PropertyPath path)
    {
        var result = path.ToList();

        result[0] = result[0].ToPascalCase();

        return new PropertyPath(result);
    }
}
