// ==========================================================================
//  MyDomainState.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.Commands;

namespace Squidex.Infrastructure.TestHelpers
{
    public class MyDomainState : IDomainState
    {
        public long Version { get; set; }
    }
}
