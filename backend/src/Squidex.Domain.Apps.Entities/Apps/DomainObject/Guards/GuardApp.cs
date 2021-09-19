﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Plans;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Text;

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject.Guards
{
    public static class GuardApp
    {
        public static void CanCreate(CreateApp command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(e =>
            {
                if (!command.Name.IsSlug())
                {
                    e(Not.ValidSlug(nameof(command.Name)), nameof(command.Name));
                }
            });
        }

        public static void CanUploadImage(UploadAppImage command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(e =>
            {
                if (command.File == null)
                {
                    e(Not.Defined(nameof(command.File)), nameof(command.File));
                }
            });
        }

        public static void CanUpdate(UpdateApp command)
        {
            Guard.NotNull(command, nameof(command));
        }

        public static void CanRemoveImage(RemoveAppImage command)
        {
            Guard.NotNull(command, nameof(command));
        }

        public static void CanUpdateAssetScripts(ConfigureAssetScripts command)
        {
            Guard.NotNull(command, nameof(command));
        }

        public static void CanUpdateSettings(UpdateAppSettings command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(e =>
            {
                var prefix = nameof(command.Settings);

                var settings = command.Settings;

                if (settings == null)
                {
                    e(Not.Defined(nameof(settings)), prefix);
                    return;
                }

                var patternsPrefix = $"{prefix}.{nameof(settings.Patterns)}";

                if (settings.Patterns == null)
                {
                    e(Not.Defined(nameof(settings.Patterns)), patternsPrefix);
                }
                else
                {
                    settings.Patterns.Foreach((pattern, index) =>
                    {
                        var patternPrefix = $"{patternsPrefix}[{index}]";

                        if (string.IsNullOrWhiteSpace(pattern.Name))
                        {
                            e(Not.Defined(nameof(pattern.Name)), $"{patternPrefix}.{nameof(pattern.Name)}");
                        }

                        if (string.IsNullOrWhiteSpace(pattern.Regex))
                        {
                            e(Not.Defined(nameof(pattern.Regex)), $"{patternPrefix}.{nameof(pattern.Regex)}");
                        }
                    });
                }

                var editorsPrefix = $"{prefix}.{nameof(settings.Editors)}";

                if (settings.Editors == null)
                {
                    e(Not.Defined(nameof(settings.Editors)), editorsPrefix);
                }
                else
                {
                    settings.Editors.Foreach((editor, index) =>
                    {
                        var editorPrefix = $"{editorsPrefix}[{index}]";

                        if (string.IsNullOrWhiteSpace(editor.Name))
                        {
                            e(Not.Defined(nameof(editor.Name)), $"{editorPrefix}.{nameof(editor.Name)}");
                        }

                        if (string.IsNullOrWhiteSpace(editor.Url))
                        {
                            e(Not.Defined(nameof(editor.Url)), $"{editorPrefix}.{nameof(editor.Url)}");
                        }
                    });
                }
            });
        }

        public static void CanChangePlan(ChangePlan command, IAppEntity app, IAppPlansProvider appPlans)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(e =>
            {
                if (string.IsNullOrWhiteSpace(command.PlanId))
                {
                    e(Not.Defined(nameof(command.PlanId)), nameof(command.PlanId));
                    return;
                }

                if (appPlans.GetPlan(command.PlanId) == null)
                {
                    e(T.Get("apps.plans.notFound"), nameof(command.PlanId));
                }

                var plan = app.Plan;

                if (!string.IsNullOrWhiteSpace(command.PlanId) && plan != null && !plan.Owner.Equals(command.Actor))
                {
                    e(T.Get("apps.plans.notPlanOwner"));
                }
            });
        }
    }
}
