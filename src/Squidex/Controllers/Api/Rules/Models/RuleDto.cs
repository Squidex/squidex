﻿// ==========================================================================
//  RuleDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Controllers.Api.Rules.Models
{
    public sealed class RuleDto
    {
        /// <summary>
        /// The id of the rule.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The user that has created the rule.
        /// </summary>
        [Required]
        public RefToken CreatedBy { get; set; }

        /// <summary>
        /// The user that has updated the rule.
        /// </summary>
        [Required]
        public RefToken LastModifiedBy { get; set; }

        /// <summary>
        /// The date and time when the rule has been created.
        /// </summary>
        public Instant Created { get; set; }

        /// <summary>
        /// The date and time when the rule has been modified last.
        /// </summary>
        public Instant LastModified { get; set; }

        /// <summary>
        /// The version of the rule.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// The trigger properties.
        /// </summary>
        [Required]
        public RuleTriggerDto Trigger { get; set; }

        /// <summary>
        /// The action properties.
        /// </summary>
        [Required]
        public RuleActionDto Action { get; set; }

        /// <summary>
        /// Determines if the rule is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}
