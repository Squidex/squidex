// ==========================================================================
//  IMigration.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Infrastructure.Migrations
{
    public interface IMigration
    {
        int FromVersion { get; }

        int ToVersion { get; }

        Task UpdateAsync();
    }
}
