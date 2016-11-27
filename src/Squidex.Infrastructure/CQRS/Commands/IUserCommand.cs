// ==========================================================================
//  IUserCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.CQRS.Commands
{
    public interface IUserCommand : ICommand
    {
        string UserId { get; set; }
    }
}
