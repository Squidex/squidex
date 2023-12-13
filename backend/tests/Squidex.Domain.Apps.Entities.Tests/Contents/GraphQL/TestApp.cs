// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL;

public static class TestApp
{
    public static readonly NamedId<DomainId> DefaultId = NamedId.Of(DomainId.NewGuid(), "my-app");

    public static readonly App Default =
        new App
        {
            Id = DefaultId.Id,
            Created = default,
            CreatedBy = RefToken.User("42"),
            Languages = LanguagesConfig.English.Set(Language.GermanGermany),
            LastModified = default,
            LastModifiedBy = RefToken.User("42"),
            Name = DefaultId.Name,
        };
}
