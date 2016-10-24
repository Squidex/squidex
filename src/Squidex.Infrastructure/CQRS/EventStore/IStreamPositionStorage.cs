// ==========================================================================
//  IStreamPositionStorage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
namespace Squidex.Infrastructure.CQRS.EventStore
{
    public interface IStreamPositionStorage
    {
        int? ReadPosition();

        void WritePosition(int position);
    }
}