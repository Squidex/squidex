// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public interface IAppEntity :
        IEntity,
        IEntityWithCreatedBy,
        IEntityWithLastModifiedBy,
        IEntityWithVersion
    {
        string Name { get; }

        string? Label { get; }

        string? Description { get; }

        Roles Roles { get; }

        AppPlan? Plan { get; }

        AppImage? Image { get; }

        AppClients Clients { get; }

        AppSettings Settings { get; }

        AppContributors Contributors { get; }

        AssetScripts AssetScripts { get; }

        LanguagesConfig Languages { get; }

        Workflows Workflows { get; }

        bool IsDeleted { get; }
    }
}
