// ==========================================================================
//  IModelSchemaEntity.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================
namespace PinkParrot.Read.Repositories
{
    public interface IModelSchemaEntity : ITenantEntity
    {
        string Name { get; }
    }
}
