// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.MongoDb.OData
{
    public delegate object ConvertValue(string field, object value);

    public static class ValueConversion
    {
        public static object Convert(string field, object value, ConvertValue converter = null)
        {
            if (converter == null)
            {
                return value;
            }

            return converter(field, value);
        }
    }
}
