// ==========================================================================
//  ICommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.Commands
{
    public interface ICommand
    {
        long ExpectedVersion { get; set; }
    }
}
