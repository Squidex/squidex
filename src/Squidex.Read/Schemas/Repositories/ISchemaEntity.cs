// ==========================================================================
//  ISchemaEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
namespace Squidex.Read.Schemas.Repositories
{
    public interface ISchemaEntity : IAppRefEntity, ITrackCreatedByEntity, ITrackLastModifiedByEntity
    {
        string Name { get; }
    }
}
