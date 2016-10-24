// ==========================================================================
//  ISchemaEntity.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================
namespace PinkParrot.Read.Schemas.Repositories
{
    public interface ISchemaEntity : IAppEntity
    {
        string Name { get; }
    }
}
