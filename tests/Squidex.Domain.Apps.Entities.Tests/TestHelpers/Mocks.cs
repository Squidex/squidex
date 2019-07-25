﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Security.Claims;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.TestHelpers
{
    public static class Mocks
    {
        public static IAppEntity App(NamedId<Guid> appId, params Language[] languages)
        {
            var config = LanguagesConfig.English;

            foreach (var language in languages)
            {
                config = config.Set(language);
            }

            var app = A.Fake<IAppEntity>();

            A.CallTo(() => app.Id).Returns(appId.Id);
            A.CallTo(() => app.Name).Returns(appId.Name);
            A.CallTo(() => app.LanguagesConfig).Returns(config);

            return app;
        }

        public static ISchemaEntity Schema(NamedId<Guid> appId, NamedId<Guid> schemaId, Schema schemaDef = null)
        {
            var schema = A.Fake<ISchemaEntity>();

            A.CallTo(() => schema.Id).Returns(schemaId.Id);
            A.CallTo(() => schema.AppId).Returns(appId);
            A.CallTo(() => schema.SchemaDef).Returns(schemaDef ?? new Schema(schemaId.Name));

            return schema;
        }

        public static ClaimsPrincipal FrontendUser(string role = null)
        {
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            claimsIdentity.AddClaim(new Claim(OpenIdClaims.ClientId, DefaultClients.Frontend));

            if (role != null)
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            return claimsPrincipal;
        }
    }
}
