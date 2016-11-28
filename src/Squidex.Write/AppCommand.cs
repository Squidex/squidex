// ==========================================================================
//  AppCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Write
{
    public abstract class AppCommand : SquidexCommand, IAppCommand
    {
        public Guid AppId { get; set; }
    }
}
