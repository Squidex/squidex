// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Apps.Models;

public sealed class UpdateLanguageDto
{
    /// <summary>
    /// Set the value to true to make the language the master.
    /// </summary>
    public bool? IsMaster { get; set; }

    /// <summary>
    /// Set the value to true to make the language optional.
    /// </summary>
    public bool IsOptional { get; set; }

    /// <summary>
    /// Optional fallback languages.
    /// </summary>
    public Language[]? Fallback { get; set; }

    public UpdateLanguage ToCommand(Language language)
    {
        return SimpleMapper.Map(this, new UpdateLanguage { Language = language });
    }
}
