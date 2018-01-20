// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Squidex.Infrastructure.Commands
{
    public interface ITimestampCommand : ICommand
    {
        Instant Timestamp { get; set; }
    }
}
