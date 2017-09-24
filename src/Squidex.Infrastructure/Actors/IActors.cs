// ==========================================================================
//  IActors.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.Actors
{
    public interface IActors
    {
        IActor Get(string id);
    }
}