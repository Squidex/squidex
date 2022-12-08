// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.HandleRules;

[AttributeUsage(AttributeTargets.Property)]
public sealed class EditorAttribute : Attribute
{
    public RuleFieldEditor Editor { get; }

    public EditorAttribute(RuleFieldEditor editor)
    {
        Editor = editor;
    }
}
