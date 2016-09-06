// ==========================================================================
//  IStreamPositionStorage.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

namespace PinkParrot.Infrastructure.CQRS.EventStore
{
    public interface IStreamPositionStorage
    {
        int? ReadPosition();

        void WritePosition(int position);
    }
}