// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Apps.Commands
{
    public sealed class ChangePlan : AppUpdateCommand
    {
        public bool FromCallback { get; set; }

        public string PlanId { get; set; }

        public string Referer { get; set; }
    }
}
