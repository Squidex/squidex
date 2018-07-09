// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Commands;

namespace Squidex.Infrastructure.TestHelpers
{
    public sealed class MyDomainState : IDomainState
    {
        public long Version { get; set; }

        public int Value { get; set; }
    }
}
