﻿// ==========================================================================
//  ITimestampCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using NodaTime;

namespace Squidex.Infrastructure.Commands
{
    public interface ITimestampCommand : ICommand
    {
        Instant Timestamp { get; set; }
    }
}
