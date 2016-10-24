// ==========================================================================
//  IAppEntity.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================
namespace PinkParrot.Read.Apps
{
    public interface IAppEntity : IEntity
    {
        string Name { get; }
    }
}
