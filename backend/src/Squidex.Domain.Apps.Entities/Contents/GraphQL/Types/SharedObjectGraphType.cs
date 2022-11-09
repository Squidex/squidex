// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;

internal abstract class SharedObjectGraphType<T> : ObjectGraphType<T>
{
    public override void Initialize(ISchema schema)
    {
        try
        {
            base.Initialize(schema);
        }
        catch (InvalidOperationException)
        {
            return;
        }
    }
}
