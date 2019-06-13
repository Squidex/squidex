﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class AppPatternDto
    {
        /// <summary>
        /// Unique id of the pattern.
        /// </summary>
        public Guid PatternId { get; set; }

        /// <summary>
        /// The name of the suggestion.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// The regex pattern.
        /// </summary>
        [Required]
        public string Pattern { get; set; }

        /// <summary>
        /// The regex message.
        /// </summary>
        public string Message { get; set; }

        public static AppPatternDto FromKvp(KeyValuePair<Guid, AppPattern> kvp)
        {
            return SimpleMapper.Map(kvp.Value, new AppPatternDto { PatternId = kvp.Key });
        }

        public static AppPatternDto FromCommand(AddPattern command)
        {
            return SimpleMapper.Map(command, new AppPatternDto());
        }
    }
}
