// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Infrastructure.Commands
{
    public interface IDomainObjectGrain : IGrainWithStringKey
    {
        Task<J<CommandResult>> ExecuteAsync(J<CommandRequest> request);
    }
}
