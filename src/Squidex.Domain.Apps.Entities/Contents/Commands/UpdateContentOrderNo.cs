// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Specialized;

namespace Squidex.Domain.Apps.Entities.Contents.Commands
{
    public sealed class UpdateContentOrderNo : ContentCommand
    {
        public long NewOrderNo { get; set; }
    }
}
