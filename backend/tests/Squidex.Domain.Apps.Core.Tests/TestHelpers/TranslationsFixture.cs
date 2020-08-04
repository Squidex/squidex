// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Translations;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Core.TestHelpers
{
    public class TranslationsFixture
    {
        public TranslationsFixture()
        {
            T.Setup(new ResourcesLocalizer(Texts.ResourceManager));
        }
    }
}
