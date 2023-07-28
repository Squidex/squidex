// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.Domain.Apps.Entities.TestHelpers;

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

        A.CallTo(() => app.Id).Returns(appId.Id);
        A.CallTo(() => app.Name).Returns(appId.Name);
        A.CallTo(() => app.Languages).Returns(config);
        A.CallTo(() => app.UniqueId).Returns(appId.Id);

        return app;
    }

    public static ISchemaEntity Schema(NamedId<DomainId> appId, NamedId<DomainId> schemaId)
    {
        var schemaEntity = A.Fake<ISchemaEntity>();
        var schemaDef = new Schema(schemaId.Name).Publish();

        A.CallTo(() => schemaEntity.Id).Returns(schemaId.Id);
        A.CallTo(() => schemaEntity.AppId).Returns(appId);
        A.CallTo(() => schemaEntity.SchemaDef).Returns(schemaDef);
        A.CallTo(() => schemaEntity.UniqueId).Returns(DomainId.Combine(appId, schemaId.Id));

        return schemaEntity;
    }

    public static ISchemaEntity Schema(NamedId<DomainId> appId, DomainId schemaId, Schema schemaDef)
    {
        var schemaEntity = A.Fake<ISchemaEntity>();

        A.CallTo(() => schemaEntity.Id).Returns(schemaId);
        A.CallTo(() => schemaEntity.AppId).Returns(appId);
        A.CallTo(() => schemaEntity.SchemaDef).Returns(schemaDef);
        A.CallTo(() => schemaEntity.UniqueId).Returns(DomainId.Combine(appId, schemaId));

        return schemaEntity;
    }

    public static ITeamEntity Team(DomainId teamId, string teamName)
    {
        var team = A.Fake<ITeamEntity>();

        A.CallTo(() => team.Id).Returns(teamId);
        A.CallTo(() => team.UniqueId).Returns(teamId);
        A.CallTo(() => team.Name).Returns(teamName);

        return team;
    }

    public static ClaimsPrincipal ApiUser(string? role = null, params string[] permissions)
    {
        return CreateUser(false, role, permissions);
    }

    public static ClaimsPrincipal ApiUser(string? role = null, string? permission = null)
    {
        return CreateUser(false, role, permission);
    }

    public static ClaimsPrincipal FrontendUser(string? role = null, params string[] permissions)
    {
        return CreateUser(true, role, permissions);
    }

    public static ClaimsPrincipal FrontendUser(string? role = null, string? permission = null)
    {
        return CreateUser(true, role, permission);
    }

    public static ClaimsPrincipal CreateUser(bool isFrontend, string? role, params string?[] permissions)
    {
        var claimsIdentity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        if (isFrontend)
        {
            claimsIdentity.AddClaim(new Claim(OpenIdClaims.ClientId, DefaultClients.Frontend));
        }

        if (role != null)
        {
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        foreach (var permission in permissions)
        {
            if (permission != null)
            {
                claimsIdentity.AddClaim(new Claim(SquidexClaimTypes.Permissions, permission));
            }
        }

        return claimsPrincipal;
    }
}
