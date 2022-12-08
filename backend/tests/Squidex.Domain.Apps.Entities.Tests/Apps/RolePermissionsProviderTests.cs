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

public class RolePermissionsProviderTests
{
    private readonly IAppEntity app;
    private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly RolePermissionsProvider sut;

    public RolePermissionsProviderTests()
    {
        app = Mocks.App(appId);

        sut = new RolePermissionsProvider(appProvider);
    }

    [Fact]
    public async Task Should_provide_all_permissions()
    {
        A.CallTo(() => appProvider.GetSchemasAsync(A<DomainId>._, default))
            .Returns(new List<ISchemaEntity>
            {
                Mocks.Schema(appId, NamedId.Of(DomainId.NewGuid(), "schema1")),
                Mocks.Schema(appId, NamedId.Of(DomainId.NewGuid(), "schema2"))
            });

        var actual = await sut.GetPermissionsAsync(app);

        Assert.True(actual.Contains("*"));
        Assert.True(actual.Contains("clients.read"));
        Assert.True(actual.Contains("schemas.*.update"));
        Assert.True(actual.Contains("schemas.schema1.update"));
        Assert.True(actual.Contains("schemas.schema2.update"));
    }
}
