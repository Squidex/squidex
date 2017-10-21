// ==========================================================================
//  UpdateField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Write.Schemas.Commands
{
    public sealed class UpdateField : FieldCommand
    {
        public FieldProperties Properties { get; set; }
    }
}