// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models;

[OpenApiRequest]
public sealed class UpdateAppSettingsDto
{
    /// <summary>
    /// The configured app patterns.
    /// </summary>
    [LocalizedRequired]
    public PatternDto[] Patterns { get; set; }

    /// <summary>
    /// The configured UI editors.
    /// </summary>
    [LocalizedRequired]
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
