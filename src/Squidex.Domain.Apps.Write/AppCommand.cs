// ==========================================================================
//  AppCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write
{
    public abstract class AppCommand : SquidexCommand
    {
        public NamedId<Guid> AppId { get; set; }
    }
}
