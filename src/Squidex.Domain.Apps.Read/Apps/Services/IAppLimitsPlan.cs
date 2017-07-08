// ==========================================================================
//  IAppLimitsPlan.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Domain.Apps.Read.Apps.Services
{
    public interface IAppLimitsPlan
    {
        string Id { get; }

        string Name { get; }

        string Costs { get; }

        long MaxApiCalls { get; }

        long MaxAssetSize { get; }

        int MaxContributors { get; }
    }
}