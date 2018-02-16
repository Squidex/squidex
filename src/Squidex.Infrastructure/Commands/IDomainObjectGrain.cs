// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure.Commands
{
    public interface IDomainObjectGrain : IStatefulObject<Guid>
    {
        Task<object> ExecuteAsync(IAggregateCommand command);

        Task WriteSnapshotAsync();

        long Version { get; }
    }
}