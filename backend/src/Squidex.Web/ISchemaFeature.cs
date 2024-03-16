﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Web
{
    public interface ISchemaFeature
    {
        ISchemaEntity Schema { get; }
    }
}
