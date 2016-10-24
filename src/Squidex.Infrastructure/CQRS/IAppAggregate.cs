// ==========================================================================
//  IAppAggregate.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.CQRS
{
    public interface IAppAggregate : IAggregate
    {
        Guid AppId { get; }
    }
}
