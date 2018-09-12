// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

#pragma warning disable RECS0083 // Shows NotImplementedException throws in the quick task bar

namespace Squidex.Infrastructure.Queries
{
    public abstract class FilterNodeVisitor<T>
    {
        public virtual T Visit(FilterComparison nodeIn)
        {
            throw new NotImplementedException();
        }

        public virtual T Visit(FilterJunction nodeIn)
        {
            throw new NotImplementedException();
        }

        public virtual T Visit(FilterNegate nodeIn)
        {
            throw new NotImplementedException();
        }
    }
}