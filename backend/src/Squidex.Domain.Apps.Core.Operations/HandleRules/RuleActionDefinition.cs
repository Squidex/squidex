// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public sealed class RuleActionDefinition
    {
        public Type Type { get; set; }

        public string Title { get; set; }

        public string ReadMore { get; set; }

        public string IconImage { get; set; }

        public string IconColor { get; set; }

        public string Display { get; set; }

        public string Description { get; set; }

        public List<RuleActionProperty> Properties { get; } = new List<RuleActionProperty>();
    }
}
