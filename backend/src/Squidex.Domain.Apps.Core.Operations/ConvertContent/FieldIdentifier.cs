// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Core.ConvertContent
{
    public abstract class FieldIdentifier
    {
        public static readonly FieldIdentifier ByName = new FieldByName();
        public static readonly FieldIdentifier ById = new FieldById();

        public abstract IField? GetField(IArrayField arrayField, string key);

        public abstract string GetStringKey(IField field);

        private sealed class FieldByName : FieldIdentifier
        {
            public override IField? GetField(IArrayField arrayField, string key)
            {
                return arrayField.FieldsByName.GetValueOrDefault(key);
            }

            public override string GetStringKey(IField field)
            {
                return field.Name;
            }
        }

        private sealed class FieldById : FieldIdentifier
        {
            public override IField? GetField(IArrayField arrayField, string key)
            {
                if (long.TryParse(key, out var id))
                {
                    return arrayField.FieldsById.GetValueOrDefault(id);
                }

                return null;
            }

            public override string GetStringKey(IField field)
            {
                return field.Id.ToString();
            }
        }
    }
}
