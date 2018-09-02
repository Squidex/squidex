// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Extensions.Actions
{
    public sealed class RuleElement
    {
        public Type Type { get; }

        public string Link { get; set; }

        public string Display { get; }

        public string Description { get; }

        public RuleElement(Type type, string color, string display, string description, string link = null)
        {
            Type = type;

            Display = display;
            Description = description;

            Link = link;
        }
    }
}
