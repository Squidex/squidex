// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Core
{
    public interface IFieldPartitioning
    {
        string Master { get; }

        IEnumerable<string> AllKeys { get; }

        IEnumerable<string> GetPriorities(string key);

        bool IsMaster(string key);

        bool IsOptional(string key);

        bool Contains(string key);

        string? GetName(string key);
    }
}
