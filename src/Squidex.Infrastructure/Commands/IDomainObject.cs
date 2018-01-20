// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure.Commands
{
    public interface IDomainObject : IStatefulObject<Guid>
    {
        long Version { get; }

        Task WriteAsync();
    }
}