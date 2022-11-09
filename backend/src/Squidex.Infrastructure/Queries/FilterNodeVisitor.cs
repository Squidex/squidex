// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable RECS0083 // Shows NotImplementedException throws in the quick task bar

namespace Squidex.Infrastructure.Queries;

public abstract class FilterNodeVisitor<T, TValue, TArgs>
{
    public virtual T Visit(CompareFilter<TValue> nodeIn, TArgs args)
    {
        throw new NotImplementedException();
    }

    public virtual T Visit(LogicalFilter<TValue> nodeIn, TArgs args)
    {
        throw new NotImplementedException();
    }

    public virtual T Visit(NegateFilter<TValue> nodeIn, TArgs args)
    {
        throw new NotImplementedException();
    }
}
