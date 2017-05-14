// ==========================================================================
//  IAppLimitsPlan.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Read.Apps.Services
{
    public interface IAppLimitsPlan
    {
        string Name { get; }

        long MaxApiCalls { get; }

        long MaxAssetSize { get; }

        int MaxContributors { get; }
    }
}