// ==========================================================================
//  IEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Read
{
    public interface IEntity
    {
        Guid Id { get; set; }

        DateTime Created { get; set; }

        DateTime LastModified { get; set; }
    }
}