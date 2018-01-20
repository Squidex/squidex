// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Entities
{
    public interface IUpdateableEntityWithAppRef
    {
        Guid AppId { get; set; }
    }
}
