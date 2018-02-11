// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Apps.Commands
{
    public sealed class ChangePlan : AppCommand
    {
        public bool FromCallback { get; set; }

        public string PlanId { get; set; }
    }
}
