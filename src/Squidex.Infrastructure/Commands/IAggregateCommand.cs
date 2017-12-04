﻿// ==========================================================================
//  IAggregateCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Commands
{
    public interface IAggregateCommand : ICommand
    {
        Guid AggregateId { get; }
    }
}