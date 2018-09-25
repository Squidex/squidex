// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class AppDto : IGenerateEtag
    {
        /// <summary>
        /// The name of the app.
        /// </summary>
        [Required]
        [RegularExpression("^[a-z0-9]+(\\-[a-z0-9]+)*$")]
        public string Name { get; set; }

        /// <summary>
        /// The version of the app.
        /// </summary>
        public long Version { get; set; }

        /// <summary>
        /// The id of the app.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The timestamp when the app has been created.
        /// </summary>
        public Instant Created { get; set; }

        /// <summary>
        /// The timestamp when the app has been modified last.
        /// </summary>
        public Instant LastModified { get; set; }

        /// <summary>
        /// The permission level of the user.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public AppContributorPermission Permission { get; set; }

        /// <summary>
        /// Gets the current plan name.
        /// </summary>
        public string PlanName { get; set; }

        /// <summary>
        /// Gets the next plan name.
        /// </summary>
        public string PlanUpgrade { get; set; }

        public static AppDto FromApp(IAppEntity app, string subject, IAppPlansProvider plans)
        {
            var response = SimpleMapper.Map(app, new AppDto());

            response.Permission = app.Contributors[subject];

            response.PlanName = plans.GetPlanForApp(app)?.Name;
            response.PlanUpgrade = plans.GetPlanUpgradeForApp(app)?.Name;

            return response;
        }
    }
}
