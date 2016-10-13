// ==========================================================================
//  IAppEntity.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;

namespace PinkParrot.Read
{
    public interface IAppEntity : IEntity
    {
        Guid AppId { get; set; }
    }
}