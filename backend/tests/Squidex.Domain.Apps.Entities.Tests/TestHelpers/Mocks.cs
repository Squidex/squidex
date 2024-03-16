// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.Domain.Apps.Entities.TestHelpers
{
    public static class Mocks
    {
        public static IAppEntity App(NamedId<DomainId> appId, params Language[] languages)
        {
            var config = LanguagesConfig.English;

            foreach (var language in languages)
            {
                config = config.Set(language);
            }

            var app = A.Fake<IAppEntity>();

            A.CallTo(() => app.Id)
                .Returns(appId.Id);

            A.CallTo(() => app.Name)
                .Returns(appId.Name);

            A.CallTo(() => app.Languages)
                .Returns(config);

            A.CallTo(() => app.UniqueId)
                .Returns(appId.Id);

            return app;
        }

        public static ISchemaEntity Schema(NamedId<DomainId> appId, NamedId<DomainId> schemaId, Schema? schemaDef = null)
        {
            var schema = A.Fake<ISchemaEntity>();

            A.CallTo(() => schema.Id)
                .Returns(schemaId.Id);

            A.CallTo(() => schema.AppId)
                .Returns(appId);

            A.CallTo(() => schema.SchemaDef)
                .Returns(schemaDef ?? new Schema(schemaId.Name));

            A.CallTo(() => schema.UniqueId)
                .Returns(DomainId.Combine(appId, schemaId.Id));

            return schema;
        }

        public static ClaimsPrincipal ApiUser(string? role = null)
        {
            return CreateUser(role, "api");
        }

        public static ClaimsPrincipal FrontendUser(string? role = null, string? permission = null)
        {
            return CreateUser(role, DefaultClients.Frontend, permission);
        }

        private static ClaimsPrincipal CreateUser(string? role, string client, string? permission = null)
        {
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            claimsIdentity.AddClaim(new Claim(OpenIdClaims.ClientId, client));

            if (role != null)
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            if (permission != null)
            {
                claimsIdentity.AddClaim(new Claim(SquidexClaimTypes.Permissions, permission));
            }

            return claimsPrincipal;
        }
    }
}
