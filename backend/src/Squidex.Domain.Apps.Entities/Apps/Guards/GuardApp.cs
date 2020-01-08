// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Apps.Guards
{
    public static class GuardApp
    {
        public static void CanCreate(CreateApp command)
        {
            Guard.NotNull(command);

            Validate.It(() => "Cannot create app.", e =>
            {
                if (!command.Name.IsSlug())
                {
                    e(Not.ValidSlug("Name"), nameof(command.Name));
                }
            });
        }

        public static void CanUploadImage(UploadAppImage command)
        {
            Guard.NotNull(command);

            Validate.It(() => "Cannot upload image.", e =>
            {
                if (command.File == null)
                {
                    e(Not.Defined("File"), nameof(command.File));
                }
            });
        }

        public static void CanUpdate(UpdateApp command)
        {
            Guard.NotNull(command);
        }

        public static void CanRemoveImage(RemoveAppImage command)
        {
            Guard.NotNull(command);
        }

        public static void CanChangePlan(ChangePlan command, AppPlan? plan, IAppPlansProvider appPlans)
        {
            Guard.NotNull(command);

            Validate.It(() => "Cannot change plan.", e =>
            {
                if (string.IsNullOrWhiteSpace(command.PlanId))
                {
                    e(Not.Defined("Plan id"), nameof(command.PlanId));
                    return;
                }

                if (appPlans.GetPlan(command.PlanId) == null)
                {
                    e("A plan with this id does not exist.", nameof(command.PlanId));
                }

                if (!string.IsNullOrWhiteSpace(command.PlanId) && plan != null && !plan.Owner.Equals(command.Actor))
                {
                    e("Plan can only changed from the user who configured the plan initially.");
                }
            });
        }
    }
}
