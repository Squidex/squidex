// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Apps.Models;

public sealed class EditorDto
{
    /// <summary>
    /// The name of the editor.
    /// </summary>
    [LocalizedRequired]
    public string Name { get; set; }

    /// <summary>
    /// The url to the editor.
    /// </summary>
    [LocalizedRequired]
    public string Url { get; set; }

    public static EditorDto FromDomain(Editor editor)
    {
        var result = SimpleMapper.Map(editor, new EditorDto());

        return result;
    }

    public Editor ToEditor()
    {
        return new Editor(Name, Url);
    }
}
