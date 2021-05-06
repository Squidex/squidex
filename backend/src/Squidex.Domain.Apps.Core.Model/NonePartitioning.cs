// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;

namespace Squidex.Domain.Apps.Core
{
    public sealed class NonePartitioning : IFieldPartitioning
    {
        public static readonly NonePartitioning Instance = new NonePartitioning();

        public string Master
        {
            get => string.Empty;
        }

        public IEnumerable<string> AllKeys
        {
            get => Enumerable.Empty<string>();
        }

        public string? GetName(string key)
        {
            return null;
        }

        public IEnumerable<string> GetPriorities(string key)
        {
            return Enumerable.Empty<string>();
        }

        public bool Contains(string key)
        {
            return false;
        }

        public bool IsMaster(string key)
        {
            return false;
        }

        public bool IsOptional(string key)
        {
            return false;
        }

        public override string ToString()
        {
            return "invariant value";
        }
    }
}
