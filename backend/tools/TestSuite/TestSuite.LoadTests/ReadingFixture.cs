// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using TestSuite.Fixtures;

namespace TestSuite.LoadTests
{
    public sealed class ReadingFixture : ContentQueryFixture1to10
    {
        public ReadingFixture()
            : base("benchmark-reading")
        {
        }
    }
}
