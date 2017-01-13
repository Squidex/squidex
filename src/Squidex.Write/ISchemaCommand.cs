// ==========================================================================
//  ISchemaCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Write
{
    public interface ISchemaCommand : IAppCommand
    {
        Guid SchemaId { get; set; }
    }
}
