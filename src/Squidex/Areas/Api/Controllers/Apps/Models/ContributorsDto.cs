// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Infrastructure;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class ContributorsDto
    {
        /// <summary>
        /// The contributors.
        /// </summary>
        [Required]
        public ContributorDto[] Contributors { get; set; }

        /// <summary>
        /// The maximum number of allowed contributors.
        /// </summary>
        public int MaxContributors { get; set; }

        public static ContributorsDto FromApp(IAppEntity app, IAppPlansProvider plans)
        {
            var plan = plans.GetPlanForApp(app);

            var contributors = app.Contributors.ToArray(x => new ContributorDto { ContributorId = x.Key, Role = x.Value });

            return new ContributorsDto { Contributors = contributors, MaxContributors = plan.MaxContributors };
        }
    }
}
