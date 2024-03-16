// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Resources;

namespace Squidex.Shared
{
    public static class Texts
    {
        private static ResourceManager? resourceManager;

        public static ResourceManager ResourceManager
        {
            get
            {
                if (resourceManager == null)
                {
                    resourceManager = new ResourceManager("Squidex.Shared.Texts", typeof(Texts).Assembly);
                }

                return resourceManager;
            }
        }
    }
}
