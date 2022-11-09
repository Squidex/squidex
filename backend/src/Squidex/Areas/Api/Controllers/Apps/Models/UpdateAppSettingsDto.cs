// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Collections;

namespace Squidex.Areas.Api.Controllers.Apps.Models;

public sealed class UpdateAppSettingsDto
{
    /// <summary>
    /// The configured app patterns.
    /// </summary>
    [Required]
    public PatternDto[] Patterns { get; set; }

    /// <summary>
    /// The configured UI editors.
    /// </summary>
    [Required]
    public EditorDto[] Editors { get; set; }

    /// <summary>
    /// Hide the scheduler for content items.
    /// </summary>
    public bool HideScheduler { get; set; }

    /// <summary>
    /// Hide the datetime mode button.
    /// </summary>
    public bool HideDateTimeModeButton { get; set; }

    public UpdateAppSettings ToCommand()
    {
        return new UpdateAppSettings
        {
            Settings = new AppSettings
            {
                Editors = Editors?.Select(x => x.ToEditor()).ToReadonlyList()!,
                HideScheduler = HideScheduler,
                HideDateTimeModeButton = HideDateTimeModeButton,
                Patterns = Patterns?.Select(x => x.ToPattern()).ToReadonlyList()!
            }
        };
    }
}
