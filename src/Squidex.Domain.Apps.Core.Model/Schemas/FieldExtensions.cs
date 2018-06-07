// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public static class FieldExtensions
    {
        public static Schema ReorderFields(this Schema schema, List<long> ids, long? parentId = null)
        {
            if (parentId != null)
            {
                return schema.UpdateField(parentId.Value, f =>
                {
                    if (f is ArrayField arrayField)
                    {
                        return arrayField.ReorderFields(ids);
                    }

                    return f;
                });
            }

            return schema.ReorderFields(ids);
        }

        public static Schema DeleteField(this Schema schema, long fieldId, long? parentId = null)
        {
            if (parentId != null)
            {
                return schema.UpdateField(parentId.Value, f =>
                {
                    if (f is ArrayField arrayField)
                    {
                        return arrayField.DeleteField(fieldId);
                    }

                    return f;
                });
            }

            return schema.DeleteField(fieldId);
        }

        public static Schema LockField(this Schema schema, long fieldId, long? parentId = null)
        {
            if (parentId != null)
            {
                return schema.UpdateField(parentId.Value, f =>
                {
                    if (f is ArrayField arrayField)
                    {
                        return arrayField.UpdateField(fieldId, n => n.Lock());
                    }

                    return f;
                });
            }

            return schema.UpdateField(fieldId, f => f.Lock());
        }

        public static Schema HideField(this Schema schema, long fieldId, long? parentId = null)
        {
            if (parentId != null)
            {
                return schema.UpdateField(parentId.Value, f =>
                {
                    if (f is ArrayField arrayField)
                    {
                        return arrayField.UpdateField(fieldId, n => n.Hide());
                    }

                    return f;
                });
            }

            return schema.UpdateField(fieldId, f => f.Hide());
        }

        public static Schema ShowField(this Schema schema, long fieldId, long? parentId = null)
        {
            if (parentId != null)
            {
                return schema.UpdateField(parentId.Value, f =>
                {
                    if (f is ArrayField arrayField)
                    {
                        return arrayField.UpdateField(fieldId, n => n.Show());
                    }

                    return f;
                });
            }

            return schema.UpdateField(fieldId, f => f.Show());
        }

        public static Schema EnableField(this Schema schema, long fieldId, long? parentId = null)
        {
            if (parentId != null)
            {
                return schema.UpdateField(parentId.Value, f =>
                {
                    if (f is ArrayField arrayField)
                    {
                        return arrayField.UpdateField(fieldId, n => n.Enable());
                    }

                    return f;
                });
            }

            return schema.UpdateField(fieldId, f => f.Enable());
        }

        public static Schema DisableField(this Schema schema, long fieldId, long? parentId = null)
        {
            if (parentId != null)
            {
                return schema.UpdateField(parentId.Value, f =>
                {
                    if (f is ArrayField arrayField)
                    {
                        return arrayField.UpdateField(fieldId, n => n.Disable());
                    }

                    return f;
                });
            }

            return schema.UpdateField(fieldId, f => f.Disable());
        }

        public static Schema UpdateField(this Schema schema, long fieldId, FieldProperties properties, long? parentId = null)
        {
            if (parentId != null)
            {
                return schema.UpdateField(parentId.Value, f =>
                {
                    if (f is ArrayField arrayField)
                    {
                        return arrayField.UpdateField(fieldId, n => n.Update(properties));
                    }

                    return f;
                });
            }

            return schema.UpdateField(fieldId, f => f.Update(properties));
        }
    }
}
