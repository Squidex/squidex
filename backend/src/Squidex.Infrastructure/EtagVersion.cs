﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure
{
    public static class EtagVersion
    {
        public const long NotFound = long.MinValue;

        public const long Auto = -3;

        public const long Any = -2;

        public const long Empty = -1;
    }
}
