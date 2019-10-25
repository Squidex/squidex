﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities
{
    public interface IUpdateableEntityWithLastModifiedBy
    {
        RefToken LastModifiedBy { get; set; }
    }
}
