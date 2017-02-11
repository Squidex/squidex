// ==========================================================================
//  SquidexCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;

namespace Squidex.Write
{
    public abstract class SquidexCommand : ICommand
    {
        public RefToken Actor { get; set; }
    }
}
