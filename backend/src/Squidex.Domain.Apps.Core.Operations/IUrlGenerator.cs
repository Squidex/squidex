// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core
{
    public interface IUrlGenerator
    {
        string AppSettingsUI(NamedId<Guid> appId);

        string AssetsUI(NamedId<Guid> appId);

        string BackupsUI(NamedId<Guid> appId);

        string ClientsUI(NamedId<Guid> appId);

        string ContentsUI(NamedId<Guid> appId);

        string ContributorsUI(NamedId<Guid> appId);

        string RulesUI(NamedId<Guid> appId);

        string SchemasUI(NamedId<Guid> appId);

        string WorkflowsUI(NamedId<Guid> appId);
    }
}
