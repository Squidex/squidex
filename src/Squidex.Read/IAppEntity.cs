// ==========================================================================
//  IAppEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Read
{
    public interface IAppEntity : IEntity
    {
        Guid AppId { get; set; }
    }
}