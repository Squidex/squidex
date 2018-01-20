// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing.Grains.Messages
{
    public sealed class ResetConsumerMessage
    {
        public string ConsumerName { get; set; }
    }
}
