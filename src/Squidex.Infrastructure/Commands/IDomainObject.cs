// ==========================================================================
//  IDomainObjectBase.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure.Commands
{
    public interface IDomainObject : IStatefulObject<Guid>
    {
        long Version { get; }

        Task WriteAsync(ISemanticLog log);
    }
}