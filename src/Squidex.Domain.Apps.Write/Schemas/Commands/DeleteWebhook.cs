// ==========================================================================
//  DeleteWebhook.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Write.Schemas.Commands
{
    public class DeleteWebhook : SchemaAggregateCommand
    {
        public Guid Id { get; set; }
    }
}
