// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Collections;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class UpdateAppSettingsDto
    {
        /// <summary>
        /// The configured app patterns.
        /// </summary>
        [Required]
        public List<PatternDto> Patterns { get; set; }

        /// <summary>
        /// The configured UI editors.
        /// </summary>
        [Required]
        public List<EditorDto> Editors { get; set; }

        /// <summary>
        /// Hide the scheduler for content items.
        /// </summary>
        public bool HideScheduler { get; set; }

        public UpdateAppSettings ToCommand()
        {
            return new UpdateAppSettings
            {
                Settings = new AppSettings
                {
                    HideScheduler = HideScheduler,
                    Patterns =
                        Patterns?.Select(x => new Pattern(x.Name, x.Regex)
                        {
                            Message = x.Message
                        }).ToImmutableList()!,
                    Editors =
                        Editors?.Select(x => new Editor(x.Name, x.Url)).ToImmutableList()!
                }
            };
        }
    }
}
