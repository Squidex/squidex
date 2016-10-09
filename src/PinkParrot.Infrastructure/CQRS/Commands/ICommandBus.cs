// ==========================================================================
//  ICommandBus.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;

namespace PinkParrot.Infrastructure.CQRS.Commands
{
    public interface ICommandBus
    {
        Task PublishAsync(ICommand command);
    }
}
