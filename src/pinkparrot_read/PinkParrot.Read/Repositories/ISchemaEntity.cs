// ==========================================================================
//  ISchemaEntity.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================
namespace PinkParrot.Read.Repositories
{
    public interface ISchemaEntity : ITenantEntity
    {
        string Name { get; }
    }
}
