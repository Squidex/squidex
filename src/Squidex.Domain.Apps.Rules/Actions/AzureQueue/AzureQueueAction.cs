// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Rules.Action.AzureQueue
{
    public sealed class AzureQueueAction : RuleAction
    {
        [Required]
        [Display(Name = "Connection String", Description = "The connection string to the storage account.")]
        public string ConnectionString { get; set; }

        [Required]
        [Display(Name = "Queue", Description = "The name of the queue.")]
        public string Queue { get; set; }

        protected override IEnumerable<ValidationError> CustomValidate()
        {
            if (!string.IsNullOrWhiteSpace(Queue) && !Regex.IsMatch(Queue, "^[a-z][a-z0-9]{2,}(\\-[a-z0-9]+)*$"))
            {
                yield return new ValidationError("Queue must be valid azure queue name.", nameof(Queue));
            }
        }
    }
}
