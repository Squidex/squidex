// ==========================================================================
//  IEntity.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;

namespace PinkParrot.Read.Schemas.Repositories
{
    public interface IEntity
    {
        Guid Id { get; set; }

        DateTime Created { get; set; }

        DateTime LastModified { get; set; }
    }
}