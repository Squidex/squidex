// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Assets
{
    public struct Range
    {
        public readonly long Offset;

        public readonly long Length;

        public long Start
        {
            get { return Offset; }
        }

        public long End
        {
            get { return Offset + Length - 1; }
        }

        public bool IsDefined
        {
            get { return Offset > 0 && Length > 0; }
        }

        public Range(long offset, long length)
        {
            Offset = offset;

            Length = length;
        }
    }
}
