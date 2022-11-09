// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Commands;

public interface ICommandBus
{
    Task<CommandContext> PublishAsync(ICommand command,
        CancellationToken ct);
}
