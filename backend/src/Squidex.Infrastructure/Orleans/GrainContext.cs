// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Squidex.Infrastructure.Orleans
{
    [Serializable]
    public class GrainContext : ISerializable
    {
        public CultureInfo Culture { get; private set; }

        public CultureInfo CultureUI { get; private set; }

        private GrainContext()
        {
        }

        protected GrainContext(SerializationInfo info, StreamingContext context)
        {
            Culture = CultureInfo.GetCultureInfo(info.GetString(nameof(Culture))!);
            CultureUI = CultureInfo.GetCultureInfo(info.GetString(nameof(CultureUI))!);
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Culture), Culture.Name);
            info.AddValue(nameof(CultureUI), CultureUI.Name);
        }

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
            if (Culture != null)
            {
                CultureInfo.CurrentCulture = Culture;
                CultureInfo.CurrentUICulture = CultureUI;
            }
        }
    }
}
