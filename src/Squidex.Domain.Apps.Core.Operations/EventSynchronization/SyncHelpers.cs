// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.EventSynchronization
{
    public static class SyncHelpers
    {
        public static bool BoolEquals(this bool lhs, bool? rhs)
        {
            return lhs == (rhs ?? false);
        }

        public static bool StringEquals(this string lhs, string rhs)
        {
            return string.Equals(lhs ?? string.Empty, rhs ?? string.Empty, StringComparison.Ordinal);
        }

        public static bool TypeEquals(this object lhs, object rhs)
        {
            return lhs.GetType() == rhs.GetType();
        }

        public static bool EqualsJson<T>(this T lhs, T rhs, IJsonSerializer serializer)
        {
            var lhsJson = serializer.Serialize(lhs);
            var rhsJson = serializer.Serialize(rhs);

            return string.Equals(lhsJson, rhsJson);
        }
    }
}
