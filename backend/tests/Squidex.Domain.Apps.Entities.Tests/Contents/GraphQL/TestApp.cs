// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public static class TestApp
    {
        public static readonly NamedId<DomainId> DefaultId = NamedId.Of(DomainId.NewGuid(), "my-app");

        public static readonly IAppEntity Default;

        static TestApp()
        {
            Default = Mocks.App(DefaultId, Language.DE, Language.GermanGermany);
        }
    }
}
