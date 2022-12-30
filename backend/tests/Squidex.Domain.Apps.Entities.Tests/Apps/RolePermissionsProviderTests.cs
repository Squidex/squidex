// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;

#pragma warning disable xUnit2017 // Do not use Contains() to check if a value exists in a collection

namespace Squidex.Domain.Apps.Entities.Apps;

public class RolePermissionsProviderTests : GivenContext
{
    private readonly RolePermissionsProvider sut;

    public RolePermissionsProviderTests()
    {
        sut = new RolePermissionsProvider(AppProvider);
    }

    [Fact]
    public async Task Should_provide_all_permissions()
    {
        A.CallTo(() => AppProvider.GetSchemasAsync(A<DomainId>._, default))
            .Returns(new List<ISchemaEntity>
            {
                Mocks.Schema(AppId, NamedId.Of(DomainId.NewGuid(), "schema1")),
                Mocks.Schema(AppId, NamedId.Of(DomainId.NewGuid(), "schema2"))
            });

        var actual = await sut.GetPermissionsAsync(App);

        Assert.True(actual.Contains("*"));
        Assert.True(actual.Contains("clients.read"));
        Assert.True(actual.Contains("schemas.*.update"));
        Assert.True(actual.Contains("schemas.schema1.update"));
        Assert.True(actual.Contains("schemas.schema2.update"));
    }
}
