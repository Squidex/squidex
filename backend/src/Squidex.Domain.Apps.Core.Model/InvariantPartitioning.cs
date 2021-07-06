// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Core
{
    public sealed class InvariantPartitioning : IFieldPartitioning
    {
        public static readonly InvariantPartitioning Instance = new InvariantPartitioning();
        public static readonly string Key = "iv";
        public static readonly string Name = "Invariant";
        public static readonly string Description = "invariant value";

        public string Master
        {
            get => Key;
        }

        public IEnumerable<string> AllKeys
        {
            get { yield return Key; }
        }

        public string? GetName(string key)
        {
            if (Contains(key))
            {
                return Name;
            }

            return null;
        }

        public IEnumerable<string> GetPriorities(string key)
        {
            if (Contains(key))
            {
                yield return Key;
            }

            yield break;
        }

        public bool Contains(string key)
        {
            return Equals(Key, key);
        }

        public bool IsMaster(string key)
        {
            return Contains(key);
        }

        public bool IsOptional(string key)
        {
            return false;
        }

        public override string ToString()
        {
            return Description;
        }
    }
}
