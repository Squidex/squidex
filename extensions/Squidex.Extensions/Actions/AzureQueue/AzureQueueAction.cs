﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.AzureQueue
{
    [RuleAction(
        Title = "Azure Queue",
        IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 32 32'><path d='M.011 16L0 6.248l12-1.63V16zM14 4.328L29.996 2v14H14zM30 18l-.004 14L14 29.75V18zM12 29.495L.01 27.851.009 18H12z'/></svg>",
        IconColor = "#0d9bf9",
        Display = "Send to Azure Queue",
        Description = "Send an event to azure queue storage.",
        ReadMore = "https://azure.microsoft.com/en-us/services/storage/queues/")]
    public sealed class AzureQueueAction : RuleAction
    {
        [Required]
        [Display(Name = "Connection", Description = "The connection string to the storage account.")]
        [DataType(DataType.Text)]
        [Formattable]
        public string ConnectionString { get; set; }

        [Required]
        [Display(Name = "Queue", Description = "The name of the queue.")]
        [DataType(DataType.Text)]
        [Formattable]
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
