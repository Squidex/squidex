// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Squidex.Infrastructure.Net
{
    public sealed class IPAddressComparer : IComparer<IPAddress>
    {
        public static readonly IPAddressComparer Instance = new IPAddressComparer();

        private IPAddressComparer()
        {
        }

        public int Compare([AllowNull] IPAddress x, [AllowNull] IPAddress y)
        {
            if (x == null || y == null)
            {
                return 0;
            }

            var lbytes = x.GetAddressBytes();
            var rbytes = y.GetAddressBytes();

            if (lbytes.Length != rbytes.Length)
            {
                return lbytes.Length - rbytes.Length;
            }

            for (var i = 0; i < lbytes.Length; i++)
            {
                if (lbytes[i] != rbytes[i])
                {
                    return lbytes[i] - rbytes[i];
                }
            }

            return 0;
        }
    }
}
