// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Orleans
{
    public interface IActivationLimit
    {
        void SetLimit(int activations, TimeSpan lifetime);

        void ReportIAmAlive();

        void ReportIAmDead();
    }
}
