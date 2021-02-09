// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;

namespace Squidex.Web.GraphQL
{
    public sealed class DummySchema : Schema
    {
        public DummySchema()
        {
            Query = new ObjectGraphType();
        }
    }
}
