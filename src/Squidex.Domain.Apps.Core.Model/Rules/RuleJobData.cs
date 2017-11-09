// ==========================================================================
//  RuleJobData.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Squidex.Domain.Apps.Core.Rules
{
    public sealed class RuleJobData : Dictionary<string, JToken>
    {
    }
}
