// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
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

        public static void CanChangePlan(ChangePlan command, IAppEntity app, IAppPlansProvider appPlans)
        {
            Guard.NotNull(command, nameof(command));

            var plan = app.Plan;

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

                if (!string.IsNullOrWhiteSpace(command.PlanId) && plan != null && !plan.Owner.Equals(command.Actor))
                {
                    e(T.Get("apps.plans.notPlanOwner"));
                }
            });
        }
    }
}
