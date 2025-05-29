// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable EF1001 // Internal EF Core API usage.

namespace Squidex.Infrastructure;

public sealed class PrefixExtension(string prefix) : IDbContextOptionsExtension
{
    public DbContextOptionsExtensionInfo Info
        => new PrefixExtensionInfo(this);

    public string Prefix { get; } = prefix;

    public void ApplyServices(IServiceCollection services)
    {
    }

    public void Validate(IDbContextOptions options)
    {
    }

    private class PrefixExtensionInfo(IDbContextOptionsExtension extension) : DbContextOptionsExtensionInfo(extension)
    {
        public override int GetServiceProviderHashCode() => 0;

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => true;

        public override bool IsDatabaseProvider => false;

        public override string LogFragment => "PrefixExtension";

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
        }
    }
}
