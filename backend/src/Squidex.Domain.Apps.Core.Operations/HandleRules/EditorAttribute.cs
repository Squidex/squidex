// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EditorAttribute : Attribute
    {
        public RuleFieldEditor Editor { get; }

        public EditorAttribute(RuleFieldEditor editor)
        {
            Editor = editor;
        }
    }
}
