// ==========================================================================
//  ITimestampCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public interface ITimestampCommand : ICommand
    {
        DateTime Timestamp { get; set; }
    }
}
