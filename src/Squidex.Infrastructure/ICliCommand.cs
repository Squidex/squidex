// ==========================================================================
//  ICommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure
{
    public interface ICliCommand
    {
        string Name { get; }

        void Execute(string[] args);
    }
}
