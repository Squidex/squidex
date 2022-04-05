// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public sealed class RuleActionProperty
    {
        public RuleFieldEditor Editor { get; set; }

        public string Name { get; set; }

        public string Display { get; set; }

        public string? Description { get; set; }

        public string[]? Options { get; set; }

        public bool IsFormattable { get; set; }

        public bool IsRequired { get; set; }
    }
}
