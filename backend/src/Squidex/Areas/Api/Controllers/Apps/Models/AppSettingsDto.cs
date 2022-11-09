// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models;

public sealed class AppSettingsDto : Resource
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

    /// <summary>
    /// The version of the app.
    /// </summary>
    public long Version { get; set; }

    public static AppSettingsDto FromDomain(IAppEntity app, Resources resources)
    {
        var settings = app.Settings;

        var result = new AppSettingsDto
        {
            Editors = settings.Editors.Select(EditorDto.FromDomain).ToArray(),
            HideDateTimeModeButton = settings.HideDateTimeModeButton,
            HideScheduler = settings.HideScheduler,
            Patterns = settings.Patterns.Select(PatternDto.FromPattern).ToArray(),
            Version = app.Version
        };

        return result.CreateLinks(resources);
    }

    private AppSettingsDto CreateLinks(Resources resources)
    {
        var values = new { app = resources.App };

        AddSelfLink(resources.Url<AppSettingsController>(x => nameof(x.GetSettings), values));

        if (resources.CanUpdateSettings)
        {
            AddPutLink("update",
                resources.Url<AppSettingsController>(x => nameof(x.PutSettings), values));
        }

        return this;
    }
}
