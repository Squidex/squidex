// ==========================================================================
//  IActor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.Actors
{
    public interface IActor
    {
        void Tell(object message);
    }
}