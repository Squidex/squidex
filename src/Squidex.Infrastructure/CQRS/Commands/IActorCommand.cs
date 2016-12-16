// ==========================================================================
//  IActorCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.CQRS.Commands
{
    public interface IActorCommand : ICommand
    {
        RefToken Actor { get; set; }
    }
}
