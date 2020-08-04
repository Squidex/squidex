// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;

namespace Squidex.Infrastructure.Commands
{
    [Serializable]
    public sealed class GrainContext
    {
        public CultureInfo Culture { get; set; }

        public CultureInfo CultureUI { get; set; }

        public static GrainContext Create()
        {
            return new GrainContext
            {
                Culture = CultureInfo.CurrentCulture,
                CultureUI = CultureInfo.CurrentUICulture
            };
        }

        public void Use()
        {
            CultureInfo.CurrentCulture = Culture;
            CultureInfo.CurrentUICulture = CultureUI;
        }
    }
}
