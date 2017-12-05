// ==========================================================================
//  SquidexCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities
{
    public abstract class SquidexCommand : ICommand
    {
        public RefToken Actor { get; set; }

        public long? ExpectedVersion { get; set; }
    }
}
