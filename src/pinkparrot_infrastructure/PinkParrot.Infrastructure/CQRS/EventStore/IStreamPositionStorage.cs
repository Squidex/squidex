// ==========================================================================
//  IStreamPositionStorage.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using EventStore.ClientAPI;

namespace PinkParrot.Infrastructure.CQRS.EventStore
{
    public interface IStreamPositionStorage
    {
        Position? ReadPosition();

        void WritePosition(Position position);
    }
}