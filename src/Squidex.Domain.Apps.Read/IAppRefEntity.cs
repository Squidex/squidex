// ==========================================================================
//  IAppRefEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Read
{
    public interface IAppRefEntity : IEntity
    {
        Guid AppId { get; set; }
    }
}