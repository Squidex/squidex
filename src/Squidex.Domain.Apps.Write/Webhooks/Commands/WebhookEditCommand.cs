// ==========================================================================
//  WebhookEditCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Webhooks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Webhooks.Commands
{
    public abstract class WebhookEditCommand : WebhookAggregateCommand, IValidatable
    {
        private List<WebhookSchema> schemas = new List<WebhookSchema>();

        public Uri Url { get; set; }

        public List<WebhookSchema> Schemas
        {
            get
            {
                return schemas ?? (schemas = new List<WebhookSchema>());
            }
            set
            {
                schemas = value;
            }
        }

        public virtual void Validate(IList<ValidationError> errors)
        {
            if (Url == null || !Url.IsAbsoluteUri)
            {
                errors.Add(new ValidationError("Url must be specified and absolute.", nameof(Url)));
            }
        }
    }
}
