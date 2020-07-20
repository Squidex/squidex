// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure
{
    public readonly struct BytesRange
    {
        public readonly long? From;

        public readonly long? To;

        public long Length
        {
            get
            {
                if (To < 0 || From < 0)
                {
                    return 0;
                }

                var result = (To ?? long.MaxValue) - (From ?? 0);

                if (result == long.MaxValue)
                {
                    return long.MaxValue;
                }

                return Math.Max(0, result + 1);
            }
        }

        public bool IsDefined
        {
            get { return (From >= 0 || To >= 0) && Length > 0; }
        }

        public BytesRange(long? from, long? to)
        {
            From = from;

            To = to;
        }

        public override string? ToString()
        {
            if (Length == 0)
            {
                return null;
            }

            if (From.HasValue || To.HasValue)
            {
                return $"bytes={From}-{To}";
            }

            return null;
        }
    }
}
