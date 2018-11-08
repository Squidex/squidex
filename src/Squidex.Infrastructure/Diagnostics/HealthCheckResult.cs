// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Infrastructure.Diagnostics
{
    public sealed class HealthCheckResult
    {
        public bool IsValid { get; }

        public string Description { get; }

        public Dictionary<string, object> Data { get; }

        public HealthCheckResult(bool isValid, string description = null, Dictionary<string, object> data = null)
        {
            IsValid = isValid;
            Data = data;
            Description = description;
        }
    }
}