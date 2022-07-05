// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans;

namespace Squidex.Infrastructure.Commands
{
    public interface IDomainObjectGrain : IGrainWithStringKey
    {
        Task<CommandResult> ExecuteAsync(IAggregateCommand command);
    }
}
