// ==========================================================================
//  IAppEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
namespace Squidex.Read.Apps
{
    public interface IAppEntity : IEntity
    {
        string Name { get; }
    }
}
