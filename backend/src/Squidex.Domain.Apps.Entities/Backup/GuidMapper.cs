// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Backup
{
    internal sealed class GuidMapper
    {
        private static readonly int GuidLength = Guid.Empty.ToString().Length;
        private readonly Dictionary<Guid, Guid> oldToNewGuid = new Dictionary<Guid, Guid>();
        private readonly Dictionary<Guid, Guid> newToOldGuid = new Dictionary<Guid, Guid>();
        private readonly Dictionary<string, string> strings = new Dictionary<string, string>();

        public Guid OldGuid(Guid newGuid)
        {
            return newToOldGuid.GetOrCreate(newGuid, x => x);
        }

        public string? NewGuidOrNull(string value)
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

        private bool TryGenerateNewGuidString(string value, [MaybeNullWhen(false)] out string result)
        {
            if (value.Length == GuidLength)
            {
                if (strings.TryGetValue(value, out result!))
                {
                    return true;
                }

                if (Guid.TryParse(value, out var guid))
                {
                    var newGuid = GenerateNewGuid(guid);

                    strings[value] = result = newGuid.ToString();

                    return true;
                }
            }

            result = null!;

            return false;
        }

        private bool TryGenerateNewNamedId(string value, [MaybeNullWhen(false)] out string result)
        {
            if (value.Length > GuidLength)
            {
                if (strings.TryGetValue(value, out result!))
                {
                    return true;
                }

                if (NamedId<Guid>.TryParse(value, Guid.TryParse, out var namedId))
                {
                    var newGuid = GenerateNewGuid(namedId.Id);

                    strings[value] = result = NamedId.Of(newGuid, namedId.Name).ToString();

                    return true;
                }
            }

            result = null!;

            return false;
        }

        private Guid GenerateNewGuid(Guid oldGuid)
        {
            if (oldGuid == Guid.Empty)
            {
                return Guid.Empty;
            }

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
