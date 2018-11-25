// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Backup
{
    internal sealed class GuidMapper
    {
        private static readonly int GuidLength = Guid.Empty.ToString().Length;
        private readonly Dictionary<Guid, Guid> oldToNewGuid = new Dictionary<Guid, Guid>();
        private readonly Dictionary<Guid, Guid> newToOldGuid = new Dictionary<Guid, Guid>();

        public Guid OldGuid(Guid newGuid)
        {
            return newToOldGuid.GetOrDefault(newGuid);
        }

        public string NewGuidOrNull(string value)
        {
            if (TryGenerateNewGuidString(value, out var result) || TryGenerateNewNamedId(value, out result))
            {
                return result;
            }

            return null;
        }

        public string NewGuidOrValue(string value)
        {
            if (TryGenerateNewGuidString(value, out var result) || TryGenerateNewNamedId(value, out result))
            {
                return result;
            }

            return value;
        }

        private bool TryGenerateNewGuidString(string value, out string result)
        {
            result = null;

            if (value.Length == GuidLength)
            {
                if (Guid.TryParse(value, out var guid))
                {
                    var newGuid = GenerateNewGuid(guid);

                    result = newGuid.ToString();

                    return true;
                }
            }

            return false;
        }

        private bool TryGenerateNewNamedId(string value, out string result)
        {
            result = null;

            if (value.Length > GuidLength && value[GuidLength] == ',')
            {
                if (Guid.TryParse(value.Substring(0, GuidLength), out var guid))
                {
                    var newGuid = GenerateNewGuid(guid);

                    result = newGuid + value.Substring(GuidLength);

                    return true;
                }
            }

            return false;
        }

        private Guid GenerateNewGuid(Guid oldGuid)
        {
            return oldToNewGuid.GetOrAdd(oldGuid, GuidGenerator);
        }

        private Guid GuidGenerator(Guid oldGuid)
        {
            var newGuid = Guid.NewGuid();

            newToOldGuid[newGuid] = oldGuid;

            return newGuid;
        }
    }
}
