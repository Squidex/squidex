﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

#pragma warning disable xUnit2017 // Do not use Contains() to check if a value exists in a collection

namespace Squidex.Domain.Apps.Entities.Apps
{
    public class RolePermissionsProviderTests
    {
        private readonly IAppEntity app;
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly RolePermissionsProvider sut;

        public RolePermissionsProviderTests()
        {
            app = Mocks.App(appId);

            sut = new RolePermissionsProvider(appProvider);
        }

        [Fact]
        public async Task Should_provide_all_permissions()
        {
            A.CallTo(() => appProvider.GetSchemasAsync(A<Guid>.Ignored))
                .Returns(new List<ISchemaEntity>
                {
                    Mocks.Schema(appId, NamedId.Of(Guid.NewGuid(), "schema1")),
                    Mocks.Schema(appId, NamedId.Of(Guid.NewGuid(), "schema2"))
                });

            var result = await sut.GetPermissionsAsync(app);

            Assert.True(result.Contains("*"));
            Assert.True(result.Contains("clients.read"));
            Assert.True(result.Contains("schemas.*.update"));
            Assert.True(result.Contains("schemas.schema1.update"));
            Assert.True(result.Contains("schemas.schema2.update"));
        }
    }
}
