// ==========================================================================
//  IAppCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.CQRS.Commands;

namespace Squidex.Write
{
    public interface IAppCommand : ICommand
    {
        Guid AppId { get; set; }
    }
}