// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Commands;

public interface IAggregate
{
    Task<CommandResult> ExecuteAsync(IAggregateCommand command,
        CancellationToken ct);
}
