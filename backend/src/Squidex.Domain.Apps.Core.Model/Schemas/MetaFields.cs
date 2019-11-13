// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Reflection;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public static class MetaFields
    {
        private static readonly HashSet<string> AllList = new HashSet<string>();

        public static ISet<string> All
        {
            get { return AllList; }
        }

        public static readonly string Id = "meta.id";
        public static readonly string Created = "meta.created";
        public static readonly string CreatedByAvatar = "meta.createdBy.avatar";
        public static readonly string CreatedByName = "meta.createdBy.name";
        public static readonly string LastModified = "meta.lastModified";
        public static readonly string LastModifiedByAvatar = "meta.lastModifiedBy.avatar";
        public static readonly string LastModifiedByName = "meta.lastModifiedBy.name";
        public static readonly string Status = "meta.status";
        public static readonly string StatusColor = "meta.status.color";
        public static readonly string Version = "meta.version";

        static MetaFields()
        {
            foreach (var field in typeof(MetaFields).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType == typeof(string))
                {
                    var value = field.GetValue(null) as string;

                    AllList.Add(value!);
                }
            }
        }
    }
}
